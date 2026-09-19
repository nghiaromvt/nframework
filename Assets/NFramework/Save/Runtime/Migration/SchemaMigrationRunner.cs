using System;
using System.Collections.Generic;

namespace NFramework.Save
{
    public static class SchemaMigrationRunner
    {
        public static byte[] Migrate(ISaveable saveable, SaveEntry entry, ISerializer serializer)
        {
            if (saveable == null)
            {
                throw new ArgumentNullException(nameof(saveable));
            }

            if (entry == null || entry.Data == null)
            {
                return null;
            }

            if (entry.SchemaVersion == saveable.SchemaVersion)
            {
                return entry.Data;
            }

            if (entry.SchemaVersion > saveable.SchemaVersion)
            {
                throw new InvalidOperationException(
                    $"Save entry '{entry.Key}' has version {entry.SchemaVersion}, which is newer than supported version {saveable.SchemaVersion}.");
            }

            int currentVersion = entry.SchemaVersion;
            byte[] currentData = entry.Data;
            IReadOnlyList<IMigration> migrations = saveable.Migrations;

            if (migrations == null || migrations.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Save entry '{entry.Key}' requires migration from v{currentVersion} to v{saveable.SchemaVersion}, but no migrations are provided.");
            }

            while (currentVersion < saveable.SchemaVersion)
            {
                IMigration step = null;
                for (int i = 0; i < migrations.Count; i++)
                {
                    if (migrations[i].FromVersion == currentVersion)
                    {
                        step = migrations[i];
                        break;
                    }
                }

                if (step == null)
                {
                    throw new InvalidOperationException(
                        $"No migration step found for '{entry.Key}' from version {currentVersion} towards version {saveable.SchemaVersion}.");
                }

                currentData = step.Migrate(currentData, serializer);
                currentVersion = step.ToVersion;
            }

            return currentData;
        }
    }
}
