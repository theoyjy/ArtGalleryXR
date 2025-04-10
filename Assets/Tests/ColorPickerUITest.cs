using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine.UI.Extensions;
using UnityEngine.Events;


public class MockFlexibleColorPicker : FlexibleColorPicker
{
    public new UnityEvent<Color> onColorChange = new UnityEvent<Color>();

    // Prevent real logic from running
    protected void OnEnable() { } //f Corrected from OnEnable to Awake

    public void SeperateMaterials() { }

    // Optional: prevent anything else from crashing
    private void Reset() { }
}
public class ColorPickerUITests
{
    private GameObject go;
    private ColorPickerUI colorPickerUI;
    private GameObject colorPickerPanel;
    private Button openButton, confirmButton;
    private Image previewImage;
    private MockFlexibleColorPicker mockPicker;
    private MarkerColorChanger mockMarker;

    [SetUp]
    public void Setup()
    {
        // Main GameObject for component
        go = new GameObject("ColorPickerUI");
        colorPickerUI = go.AddComponent<ColorPickerUI>();

        // Open Color Button
        openButton = new GameObject("OpenButton").AddComponent<Button>();
        openButton.gameObject.AddComponent<Image>();
        colorPickerUI.openColorPickerButton = openButton;

        // Confirm Button
        confirmButton = new GameObject("ConfirmButton").AddComponent<Button>();
        confirmButton.gameObject.AddComponent<Image>();
        colorPickerUI.confirmButton = confirmButton;

        // Preview Image
        previewImage = new GameObject("Preview").AddComponent<Image>();
        colorPickerUI.selectedColorPreview = previewImage;

        // Mock MarkerColorChanger
        mockMarker = new GameObject("MockMarker").AddComponent<MarkerColorChanger>();
        colorPickerUI.canvasComponent = mockMarker.gameObject;

        // Panel and Picker (MUST be created before Start is called)
        colorPickerPanel = new GameObject("ColorPickerPanel");
        mockPicker = colorPickerPanel.AddComponent<MockFlexibleColorPicker>();
        colorPickerUI.colorPickerPanel = colorPickerPanel;
        colorPickerUI.colorPicker = mockPicker; // Must set this BEFORE calling Start

        // Call Start manually now that all is ready
        colorPickerUI.SendMessage("Start");
    }

    [TearDown]
    public void Teardown()
    {
        if (go != null) Object.DestroyImmediate(go);
        if (openButton != null) Object.DestroyImmediate(openButton.gameObject);
        if (confirmButton != null) Object.DestroyImmediate(confirmButton.gameObject);
        if (previewImage != null) Object.DestroyImmediate(previewImage.gameObject);
        if (colorPickerPanel != null) Object.DestroyImmediate(colorPickerPanel);
        if (mockMarker != null) Object.DestroyImmediate(mockMarker.gameObject);
    }

    //[Test]
    //public void Start_InitializesUIProperly()
    //{
    //    Assert.IsFalse(colorPickerPanel.activeSelf);
    //    Assert.IsFalse(confirmButton.gameObject.activeSelf);
    //    Assert.AreEqual(Color.red, colorPickerUI.SendColor);
    //}

    //[UnityTest]
    //public IEnumerator UpdateSelectedColor_UpdatesPreviewImage()
    //{
    //    Color newColor = Color.green;
    //    mockPicker.onColorChange.Invoke(newColor);
    //    yield return null;

    //    Assert.AreEqual(newColor, previewImage.color);
    //}

    //[UnityTest]
    //public IEnumerator ApplySelectedColor_UpdatesAllColorTargets()
    //{
    //    Color newColor = Color.cyan;
    //    mockPicker.onColorChange.Invoke(newColor);
    //    yield return null;

    //    colorPickerUI.SendMessage("ApplySelectedColor");
    //    yield return null;

    //    Assert.AreEqual(newColor, openButton.image.color);
    //    Assert.AreEqual(newColor, mockMarker.newColor);
    //    Assert.AreEqual(newColor, colorPickerUI.SendColor);
    //    Assert.IsFalse(colorPickerPanel.activeSelf);
    //    Assert.IsFalse(confirmButton.gameObject.activeSelf);
    //}
}

