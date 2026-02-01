using UnityEngine;

/// <summary>
/// Helper class for reading gamepad input for both players.
/// Player 1 uses Joystick 1, Player 2 uses Joystick 2.
/// Provides unified input checking that combines keyboard and gamepad.
/// </summary>
public static class GamepadInput
{
    private const float DEADZONE = 0.5f;
    
    // Track previous frame stick state for "GetKeyDown" style behavior
    private static bool[] prevHorizontalNeg = new bool[2];
    private static bool[] prevHorizontalPos = new bool[2];
    private static bool[] prevVerticalNeg = new bool[2];
    private static bool[] prevVerticalPos = new bool[2];
    
    // Track previous button states
    private static bool[] prevButtonA = new bool[2];
    private static bool[] prevButtonB = new bool[2];
    private static bool[] prevButtonX = new bool[2];
    private static bool[] prevButtonY = new bool[2];
    private static bool[] prevButtonLB = new bool[2];
    private static bool[] prevButtonRB = new bool[2];
    private static bool[] prevButtonStart = new bool[2];
    private static bool[] prevDPadUp = new bool[2];
    private static bool[] prevDPadDown = new bool[2];
    private static bool[] prevDPadLeft = new bool[2];
    private static bool[] prevDPadRight = new bool[2];
    
    private static int lastUpdateFrame = -1;
    
    /// <summary>
    /// Call this at the end of Update() to track button state changes.
    /// Safe to call multiple times - will only execute once per frame.
    /// </summary>
    public static void LateUpdate()
    {
        if (Time.frameCount == lastUpdateFrame) return;
        lastUpdateFrame = Time.frameCount;
        
        for (int p = 0; p < 2; p++)
        {
            int joy = p + 1;
            
            float h = GetJoystickAxis(joy, 0);
            float v = GetJoystickAxis(joy, 1);
            
            prevHorizontalNeg[p] = h < -DEADZONE;
            prevHorizontalPos[p] = h > DEADZONE;
            prevVerticalNeg[p] = v < -DEADZONE;
            prevVerticalPos[p] = v > DEADZONE;
            
            prevButtonA[p] = GetJoystickButton(joy, 0);
            prevButtonB[p] = GetJoystickButton(joy, 1);
            prevButtonX[p] = GetJoystickButton(joy, 2);
            prevButtonY[p] = GetJoystickButton(joy, 3);
            prevButtonLB[p] = GetJoystickButton(joy, 4);
            prevButtonRB[p] = GetJoystickButton(joy, 5);
            prevButtonStart[p] = GetJoystickButton(joy, 7);
            
            float dpadH = GetJoystickAxis(joy, 5);
            float dpadV = GetJoystickAxis(joy, 6);
            prevDPadLeft[p] = dpadH < -DEADZONE;
            prevDPadRight[p] = dpadH > DEADZONE;
            prevDPadDown[p] = dpadV < -DEADZONE;
            prevDPadUp[p] = dpadV > DEADZONE;
        }
    }
    
    // ==========================================
    // Direction Input (combines stick + D-pad)
    // ==========================================
    
    public static bool GetLeftDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        float h = GetJoystickAxis(joy, 0);
        float dpadH = GetJoystickAxis(joy, 5);
        
        bool nowLeft = h < -DEADZONE || dpadH < -DEADZONE;
        bool wasLeft = prevHorizontalNeg[playerIndex] || prevDPadLeft[playerIndex];
        
        return nowLeft && !wasLeft;
    }
    
    public static bool GetRightDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        float h = GetJoystickAxis(joy, 0);
        float dpadH = GetJoystickAxis(joy, 5);
        
        bool nowRight = h > DEADZONE || dpadH > DEADZONE;
        bool wasRight = prevHorizontalPos[playerIndex] || prevDPadRight[playerIndex];
        
        return nowRight && !wasRight;
    }
    
    public static bool GetUpDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        float v = GetJoystickAxis(joy, 1);
        float dpadV = GetJoystickAxis(joy, 6);
        
        bool nowUp = v < -DEADZONE || dpadV > DEADZONE;
        bool wasUp = prevVerticalNeg[playerIndex] || prevDPadUp[playerIndex];
        
        return nowUp && !wasUp;
    }
    
    public static bool GetDownDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        float v = GetJoystickAxis(joy, 1);
        float dpadV = GetJoystickAxis(joy, 6);
        
        bool nowDown = v > DEADZONE || dpadV < -DEADZONE;
        bool wasDown = prevVerticalPos[playerIndex] || prevDPadDown[playerIndex];
        
        return nowDown && !wasDown;
    }
    
    public static bool GetLeft(int playerIndex)
    {
        int joy = playerIndex + 1;
        float h = GetJoystickAxis(joy, 0);
        float dpadH = GetJoystickAxis(joy, 5);
        return h < -DEADZONE || dpadH < -DEADZONE;
    }
    
    public static bool GetRight(int playerIndex)
    {
        int joy = playerIndex + 1;
        float h = GetJoystickAxis(joy, 0);
        float dpadH = GetJoystickAxis(joy, 5);
        return h > DEADZONE || dpadH > DEADZONE;
    }
    
    public static bool GetUp(int playerIndex)
    {
        int joy = playerIndex + 1;
        float v = GetJoystickAxis(joy, 1);
        float dpadV = GetJoystickAxis(joy, 6);
        return v < -DEADZONE || dpadV > DEADZONE;
    }
    
    public static bool GetDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        float v = GetJoystickAxis(joy, 1);
        float dpadV = GetJoystickAxis(joy, 6);
        return v > DEADZONE || dpadV < -DEADZONE;
    }
    
    // ==========================================
    // Button Input
    // ==========================================
    
    public static bool GetButtonADown(int playerIndex)
    {
        int joy = playerIndex + 1;
        bool now = GetJoystickButton(joy, 0);
        return now && !prevButtonA[playerIndex];
    }
    
    public static bool GetButtonA(int playerIndex)
    {
        return GetJoystickButton(playerIndex + 1, 0);
    }
    
    public static bool GetButtonBDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        bool now = GetJoystickButton(joy, 1);
        return now && !prevButtonB[playerIndex];
    }
    
    public static bool GetButtonB(int playerIndex)
    {
        return GetJoystickButton(playerIndex + 1, 1);
    }
    
    public static bool GetButtonXDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        bool now = GetJoystickButton(joy, 2);
        return now && !prevButtonX[playerIndex];
    }
    
    public static bool GetButtonX(int playerIndex)
    {
        return GetJoystickButton(playerIndex + 1, 2);
    }
    
    public static bool GetButtonYDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        bool now = GetJoystickButton(joy, 3);
        return now && !prevButtonY[playerIndex];
    }
    
    public static bool GetButtonY(int playerIndex)
    {
        return GetJoystickButton(playerIndex + 1, 3);
    }
    
    public static bool GetButtonLBDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        bool now = GetJoystickButton(joy, 4);
        return now && !prevButtonLB[playerIndex];
    }
    
    public static bool GetButtonLB(int playerIndex)
    {
        return GetJoystickButton(playerIndex + 1, 4);
    }
    
    public static bool GetButtonRBDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        bool now = GetJoystickButton(joy, 5);
        return now && !prevButtonRB[playerIndex];
    }
    
    public static bool GetButtonRB(int playerIndex)
    {
        return GetJoystickButton(playerIndex + 1, 5);
    }
    
    public static bool GetButtonStartDown(int playerIndex)
    {
        int joy = playerIndex + 1;
        bool now = GetJoystickButton(joy, 7);
        return now && !prevButtonStart[playerIndex];
    }
    
    public static float GetLeftTrigger(int playerIndex)
    {
        return Mathf.Max(0, GetJoystickAxis(playerIndex + 1, 8));
    }
    
    public static float GetRightTrigger(int playerIndex)
    {
        return Mathf.Max(0, GetJoystickAxis(playerIndex + 1, 9));
    }
    
    public static bool AnyGamepadConnected()
    {
        return Input.GetJoystickNames().Length > 0;
    }
    
    public static bool IsGamepadConnected(int playerIndex)
    {
        string[] joysticks = Input.GetJoystickNames();
        if (playerIndex < joysticks.Length)
        {
            return !string.IsNullOrEmpty(joysticks[playerIndex]);
        }
        return false;
    }
    
    // ==========================================
    // Helper methods
    // ==========================================
    
    private static float GetJoystickAxis(int joystickNumber, int axisNumber)
    {
        try
        {
            // For Player 1, use the standard Horizontal/Vertical axes
            if (joystickNumber == 1)
            {
                if (axisNumber == 0) return Input.GetAxis("Horizontal");
                if (axisNumber == 1) return Input.GetAxis("Vertical");
                // For D-pad axes, try the named axes but fall back to 0
                if (axisNumber == 5 || axisNumber == 6) return Input.GetAxisRaw($"Joy{joystickNumber}Axis{axisNumber}");
            }
            
            // For Player 2, try the specific joy axis name first, then fall back
            string axisName = $"Joy{joystickNumber}Axis{axisNumber}";
            try
            {
                float val = Input.GetAxisRaw(axisName);
                return val;
            }
            catch
            {
                // If that axis doesn't exist, return 0
                return 0f;
            }
        }
        catch
        {
            return 0f;
        }
    }
    
    private static bool GetJoystickButton(int joystickNumber, int buttonNumber)
    {
        // Standard gamepad buttons map to Fire1, Fire2, Fire3 in most configurations
        // For now, use Input.GetKey with KeyCode as fallback
        KeyCode keyCode = (KeyCode)((int)KeyCode.Joystick1Button0 + (joystickNumber - 1) * 20 + buttonNumber);
        
        // Try KeyCode first
        if (Input.GetKey(keyCode))
            return true;
        
        // Fallback to Fire axes for Player 1
        if (joystickNumber == 1)
        {
            switch (buttonNumber)
            {
                case 0: return Input.GetButton("Fire1"); // A button
                case 1: return Input.GetButton("Fire2"); // B button
                case 2: return Input.GetButton("Fire3"); // X button
                // Note: Fire3 might be the only one configured, but try all
            }
        }
        
        return false;
    }
    
    public static KeyCode GetJoystickButtonKeyCode(int joystickNumber, int buttonNumber)
    {
        return (KeyCode)((int)KeyCode.Joystick1Button0 + (joystickNumber - 1) * 20 + buttonNumber);
    }
}
