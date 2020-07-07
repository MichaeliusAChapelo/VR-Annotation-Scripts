using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maps an button to a specific input command.
/// </summary>
public static class InputMapping
{
    public static bool IsRightHanded = true;

    /// <summary>
    /// Private holder for whether a button was pressed.
    /// </summary>
    private class ButtonPress
    {
        // Internal fields
        internal readonly OVRInput.RawButton Button;
        internal bool ThisFrame, PreviousFrame;

        // Internal constructor
        internal ButtonPress(OVRInput.RawButton button)
        {
            this.Button = button;
            ThisFrame = PreviousFrame = false;
        }

    }

    /// <summary>
    /// List of user-mapped input.
    /// </summary>
    public enum AnnotationInput
    {
        ExportAndClear,
        SizeUp,
        SizeDown,
        Annotate,
        DestroyAnnotation,
        ResetTransform,
        SwitchDominantHand,
        //StartMenu
    }

    /// <summary>
    /// Dictionary that maps input to buttons presses.
    /// </summary>
    private static readonly Dictionary<AnnotationInput, ButtonPress> ButtonMappings = new Dictionary<AnnotationInput, ButtonPress>()
        {
            {AnnotationInput.SizeUp, new ButtonPress(OVRInput.RawButton.Y)},
            {AnnotationInput.SizeDown, new ButtonPress(OVRInput.RawButton.X)},
            {AnnotationInput.DestroyAnnotation, new ButtonPress(OVRInput.RawButton.B)},
            {AnnotationInput.ResetTransform, new ButtonPress(OVRInput.RawButton.A)},
            {AnnotationInput.ExportAndClear, new ButtonPress(OVRInput.RawButton.Start)}, // could bring up a OVR menu with the same toggle
            {AnnotationInput.Annotate, new ButtonPress(OVRInput.RawButton.RIndexTrigger) },
            {AnnotationInput.SwitchDominantHand, new ButtonPress(OVRInput.RawButton.LIndexTrigger) },
        };

    /// <summary>
    /// Swaps two input mappings.
    /// </summary>
    private static void SwapButtons(AnnotationInput a, AnnotationInput b)
    {
        ButtonPress aButtonPress = ButtonMappings[a];
        ButtonMappings[a] = ButtonMappings[b];
        ButtonMappings[b] = aButtonPress;
    }

    /// <summary>
    /// Switches dominant hand.
    /// </summary>
    public static void SwitchDominantHand()
    {
        IsRightHanded = !IsRightHanded;
        SwapButtons(AnnotationInput.SwitchDominantHand, AnnotationInput.Annotate);
        //SwapButtons(AnnotationInput.SizeUp, AnnotationInput.DestroyAnnotation);
        //SwapButtons(AnnotationInput.SizeDown, AnnotationInput.ResetTransform);
    }

    /// <summary>
    /// Returns true if the given button corresponding to the mapped input was pressed this frame.
    /// </summary>
    /// <param name="input">Mapped input.</param>
    /// <returns></returns>
    public static bool PressedThisFrame(AnnotationInput input)
    {
        ButtonPress bp = ButtonMappings[input];
        return !bp.PreviousFrame && bp.ThisFrame;
    }

    /// <summary>
    /// Returns true if the given button corresponding to the mapped input is held this frame.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool HeldThisFrame(AnnotationInput input) { return ButtonMappings[input].ThisFrame; }

    /// <summary>
    /// Updates all the button reads this frame. Use in the beginning of an update function.
    /// </summary>
    public static void UpdateThisFrame() { foreach (ButtonPress b in ButtonMappings.Values) b.ThisFrame = OVRInput.Get(b.Button); }

    /// <summary>
    /// Updates all previous button reads from frame, readying for next frame. Use at the end of an update function.
    /// </summary>
    public static void UpdatePreviousFrame() { foreach (ButtonPress b in ButtonMappings.Values) b.PreviousFrame = b.ThisFrame; }
}

