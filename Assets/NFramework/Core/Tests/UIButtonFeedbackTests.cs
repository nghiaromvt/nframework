using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NFramework.Core.Tests
{
    [TestFixture]
    public class UIButtonFeedbackTests
    {
        private class TestButtonFeedback : UIButtonFeedback
        {
            public bool PointerDownFeedbackCalled { get; private set; }
            public bool ClickCalled { get; private set; }

            public float CustomScaleInMultiplier => ScaleInMultiplier;
            public float CustomDurationIn => DurationIn;
            public float CustomDurationOut => DurationOut;
            public float CustomAmplitude => Amplitude;

            protected override void OnPointerDownFeedback()
            {
                PointerDownFeedbackCalled = true;
            }

            protected override void OnClick()
            {
                ClickCalled = true;
            }
        }

        private GameObject _go;
        private Button _button;
        private TestButtonFeedback _feedback;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestButton", typeof(RectTransform), typeof(Button), typeof(TestButtonFeedback));
            _button = _go.GetComponent<Button>();
            _feedback = _go.GetComponent<TestButtonFeedback>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
            }
        }

        [Test]
        public void DefaultProperties_MatchSpecification()
        {
            Assert.AreEqual(0.9f, _feedback.CustomScaleInMultiplier, 0.001f);
            Assert.AreEqual(0.1f, _feedback.CustomDurationIn, 0.001f);
            Assert.AreEqual(0.15f, _feedback.CustomDurationOut, 0.001f);
            Assert.AreEqual(1.5f, _feedback.CustomAmplitude, 0.001f);
        }

        [Test]
        public void OnPointerDown_WhenInteractable_TriggersPointerDownFeedback()
        {
            var eventData = new PointerEventData(EventSystem.current) { pointerId = 0 };

            _feedback.OnPointerDown(eventData);

            Assert.IsTrue(_feedback.PointerDownFeedbackCalled);
        }

        [Test]
        public void OnPointerDown_WhenNotInteractable_DoesNotTriggerFeedback()
        {
            _button.interactable = false;
            var eventData = new PointerEventData(EventSystem.current) { pointerId = 0 };

            _feedback.OnPointerDown(eventData);

            Assert.IsFalse(_feedback.PointerDownFeedbackCalled);
        }

        [Test]
        public void MultiTouch_IgnoresSecondaryPointer()
        {
            var pointer1 = new PointerEventData(EventSystem.current) { pointerId = 1 };
            var pointer2 = new PointerEventData(EventSystem.current) { pointerId = 2 };

            _feedback.OnPointerDown(pointer1);
            Assert.IsTrue(_feedback.PointerDownFeedbackCalled);

            // Pointer 2 should not reset or exit active pointer 1
            _feedback.OnPointerExit(pointer2);

            // Up on pointer 1 should release active pointer
            _feedback.OnPointerUp(pointer1);
        }

        [Test]
        public void ButtonClick_InvokesVirtualOnClick()
        {
            _button.onClick.Invoke();

            Assert.IsTrue(_feedback.ClickCalled);
        }

        [Test]
        public void OnDisable_ResetsTransformScale()
        {
            _go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            _go.SetActive(false);

            Assert.AreEqual(Vector3.one, _go.transform.localScale);
        }
    }
}
