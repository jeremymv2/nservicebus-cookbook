using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

/// <summary>
/// Basic .NET 2 compatible registry API wrapper
/// Supports accessing WOW64 or WOW32 reg unlike standard Registry functions prior to .Net 4
/// </summary>
public class RegHelper
{
    private int WOWOption = 0; 
    private  IntPtr RootKey;

    private const int KEY_QUERY_VALUE = 0x0001;
    private const int KEY_SET_VALUE = 0x0002;
    private const int KEY_CREATE_SUB_KEY = 0x0004;
    private const int KEY_ENUMERATE_SUB_KEYS = 0x0008;
    private const int KEY_NOTIFY = 0x0010;
    private const int KEY_CREATE_LINK = 0x0020;
    private const int KEY_READ = ((STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY) & (~SYNCHRONIZE));
    private const int KEY_WRITE = ((STANDARD_RIGHTS_WRITE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY) & (~SYNCHRONIZE));
    private const int KEY_WOW64_64KEY = 0x0100;
    private const int KEY_WOW64_32KEY = 0x0200;
    private const int REG_OPTION_NON_VOLATILE = 0x0000;
    private const int REG_OPTION_VOLATILE = 0x0001;
    private const int REG_OPTION_CREATE_LINK = 0x0002;
    private const int REG_OPTION_BACKUP_RESTORE = 0x0004;
    private const int REG_NONE = 0;
    private const int REG_SZ = 1;
    private const int REG_EXPAND_SZ = 2;

    private const int REG_BINARY = 3;
    private const int REG_DWORD = 4;
    private const int REG_DWORD_LITTLE_ENDIAN = 4;
    private const int REG_DWORD_BIG_ENDIAN = 5;
    private const int REG_LINK = 6;
    private const int REG_MULTI_SZ = 7;
    private const int REG_RESOURCE_LIST = 8;
    private const int REG_FULL_RESOURCE_DESCRIPTOR = 9;
    private const int REG_RESOURCE_REQUIREMENTS_LIST = 10;
    private const int REG_QWORD = 11;
    private const int READ_CONTROL = 0x00020000;
    private const int SYNCHRONIZE = 0x00100000;

    private const int STANDARD_RIGHTS_READ = READ_CONTROL;
    private const int STANDARD_RIGHTS_WRITE = READ_CONTROL;

    private const int SUCCESS = 0;
    private const int FILE_NOT_FOUND = 2;
    private const int ACCESS_DENIED = 5;
    private const int INVALID_PARAMETER = 87;
    private const int MORE_DATA = 234;
    private const int NO_MORE_ENTRIES = 259;
    private const int MARKED_FOR_DELETION = 1018;
    private const int BUFFER_MAX_LENGTH = 2048;
        
    private static readonly IntPtr HKEY_CURRENT_USER = new IntPtr(unchecked((int)0x80000001));
    private static readonly IntPtr HKEY_LOCAL_MACHINE = new IntPtr(unchecked((int)0x80000002));
        
    private const int MaxKeyLength = 255;
    private const int MaxValueLength = 16383;

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int sam, out IntPtr phkResult);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
    private static extern int RegEnumKey(IntPtr keyBase, int index, StringBuilder nameBuffer, int bufferLength);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int RegCloseKey(IntPtr hKey);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int RegEnumValue(IntPtr hKey, int dwIndex, StringBuilder lpValueName, ref int lpcchValueName, int lpReserved, int lpType, int lpData, int lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    private static extern int RegQueryValueEx(IntPtr hKey, String lpValueName, int[] lpReserved, ref int lpType, ref int lpData, ref int lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    private static extern int RegQueryValueEx(IntPtr hKey, String lpValueName, int[] lpReserved, ref int lpType, ref long lpData, ref int lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    private static extern int RegQueryValueEx(IntPtr hKey, String lpValueName, int[] lpReserved, ref int lpType, [Out] byte[] lpData, ref int lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    private static extern int RegQueryValueEx(IntPtr hKey, String lpValueName, int[] lpReserved, ref int lpType, [Out] char[] lpData, ref int lpcbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    private static extern int RegSetValueEx(IntPtr hKey, String lpValueName, int reserved, RegistryValueKind dwType, byte[] lpData, int cbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    private static extern int RegSetValueEx(IntPtr hKey, String lpValueName, int reserved, RegistryValueKind dwType, ref int lpData, int cbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    private static extern int RegSetValueEx(IntPtr hKey, String lpValueName, int reserved, RegistryValueKind dwType, ref long lpData, int cbData);

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, BestFitMapping = false)]
    private static extern int RegSetValueEx(IntPtr hKey, String lpValueName, int reserved, RegistryValueKind dwType, String lpData, int cbData);

    private RegHelper()
    {
            
    }

    public static RegHelper LocalMachine(bool WOW64bit)
    {
        RegHelper helper = new RegHelper();
        helper.RootKey = HKEY_LOCAL_MACHINE;
        helper.WOWOption = (WOW64bit) ? KEY_WOW64_64KEY : 0; //TODO: Confirm on 32 bit OS if this should be 0 or KEY_WOW64_32KEY
        return helper;
    }

    public static RegHelper CurrentUser(bool WOW64bit)
    {
        RegHelper helper = new RegHelper();
        helper.RootKey = HKEY_CURRENT_USER;
        helper.WOWOption = (WOW64bit) ? KEY_WOW64_64KEY : 0; //TODO: Confirm on 32 bit OS if this should be 0 or KEY_WOW64_32KEY
        return helper;
    }

    public string[] GetSubKeyNames(string subKeyName)
    {
        IntPtr regKeyHandle = IntPtr.Zero;
        ArrayList keyNames = new ArrayList();
        try
        {
            if (RegOpenKeyEx(RootKey, subKeyName, 0, KEY_READ | WOWOption, out regKeyHandle) != 0)
            {
                throw new Exception("Failed to open registry key");
            }
            StringBuilder buffer = new StringBuilder(BUFFER_MAX_LENGTH);
            for (int index = 0; ; index++)
            {
                int result = RegEnumKey(regKeyHandle, index, buffer, buffer.Capacity);

                if (result == SUCCESS)
                {
                    keyNames.Add(buffer.ToString());
                    buffer.Length = 0;
                    continue;
                }

                if (result == NO_MORE_ENTRIES)
                {
                    break;
                }
                throw new Win32Exception(result);
            }
            return (string[])keyNames.ToArray(typeof(string));
        }
        finally
        {
            if (regKeyHandle != IntPtr.Zero)
            {
                RegCloseKey(regKeyHandle);
            }
        }
    }
    public string[] GetValueNames(string subKeyName)
    {
        IntPtr regKeyHandle = IntPtr.Zero;
        ArrayList keyNames = new ArrayList();
        try
        {
            if (RegOpenKeyEx(RootKey, subKeyName, 0, KEY_READ | WOWOption, out regKeyHandle) != 0)
            {
                throw new Exception("Failed to open registry key");
            }
            StringBuilder buffer = new StringBuilder(256);

            for (int index = 0; ; index++)
            {
                int bufferSize = 256;
                int result = RegEnumValue(regKeyHandle, index, buffer, ref bufferSize, 0, 0, 0, 0);
                if (result == SUCCESS)
                {
                    keyNames.Add(buffer.ToString());
                    buffer.Capacity = 256;
                    buffer.Length = 0;
                    continue;
                }
                if (result == NO_MORE_ENTRIES)
                {
                    break;
                }
                throw new Win32Exception(result);
            }
            return (string[])keyNames.ToArray(typeof(string));
        }
        finally
        {
            if (regKeyHandle != IntPtr.Zero)
            {
                RegCloseKey(regKeyHandle);
            }
        }
    }
    public object ReadValue(string subKeyName, string valueName, object defaultValue, bool doNotExpand)
    {
        Object data = defaultValue;
        int type = 0;
        int datasize = 0;
        IntPtr regKeyHandle = IntPtr.Zero;
        try
        {
            if (RegOpenKeyEx(RootKey, subKeyName, 0, KEY_READ | WOWOption, out regKeyHandle) != 0)
            {
                throw new Exception("Failed to open registry key");
            }
            int ret = RegQueryValueEx(regKeyHandle, valueName, null, ref type, (byte[])null, ref datasize);
            if (ret != 0)
            {
                if (ret != MORE_DATA)
                {
                    return data; //Error Return Default
                }
            }
            else
            {
                if (datasize < 0)
                {
                    datasize = 0;
                }
                switch (type)
                {
                    case REG_NONE:
                    case REG_DWORD_BIG_ENDIAN:
                    case REG_BINARY:
                        {
                            byte[] blob = new byte[datasize];
                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, blob, ref datasize);
                            data = blob;
                        }
                        break;
                    case REG_QWORD:
                        {
                            if (datasize > 8)
                            {
                                goto case REG_BINARY;
                            }
                            long blob = 0;
                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, ref blob, ref datasize);
                            data = blob;
                        }
                        break;
                    case REG_DWORD:
                        {
                            if (datasize > 4)
                            {
                                goto case REG_QWORD;
                            }
                            int blob = 0;
                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, ref blob, ref datasize);
                            data = blob;
                        }
                        break;

                    case REG_SZ:
                        {
                            char[] blob = new char[datasize / 2];
                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, blob, ref datasize);
                            if (blob.Length > 0 && blob[blob.Length - 1] == (char)0)
                            {
                                data = new String(blob, 0, blob.Length - 1);
                            }
                            else
                            {
                                data = new String(blob);
                            }
                        }
                        break;
                    case REG_EXPAND_SZ:
                        {
                            char[] blob = new char[datasize / 2];
                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, blob, ref datasize);
                            if (blob.Length > 0 && blob[blob.Length - 1] == (char)0)
                            {
                                data = new String(blob, 0, blob.Length - 1);
                            }
                            else
                            {
                                data = new String(blob);
                            }
                            if (!doNotExpand)
                                data = Environment.ExpandEnvironmentVariables((String)data);
                        }
                        break;
                    case REG_MULTI_SZ:
                        {
                            char[] blob = new char[datasize / 2];

                            RegQueryValueEx(regKeyHandle, valueName, null, ref type, blob, ref datasize);

                            // Ensure String is null terminated
                            if (blob.Length > 0 && blob[blob.Length - 1] != (char)0)
                            {
                                char[] newBlob = new char[checked(blob.Length + 1)];
                                for (int i = 0; i < blob.Length; i++)
                                {
                                    newBlob[i] = blob[i];
                                }
                                newBlob[newBlob.Length - 1] = (char)0;
                                blob = newBlob;
                                blob[blob.Length - 1] = (char)0;
                            }

                            IList<String> strings = new List<String>();
                            int cur = 0;
                            int len = blob.Length;

                            while (ret == 0 && cur < len)
                            {
                                int nextNull = cur;
                                while (nextNull < len && blob[nextNull] != (char)0)
                                {
                                    nextNull++;
                                }

                                if (nextNull < len)
                                {
                                    if (nextNull - cur > 0)
                                    {
                                        strings.Add(new String(blob, cur, nextNull - cur));
                                    }
                                    else
                                    {
                                        if (nextNull != len - 1)
                                            strings.Add(String.Empty);
                                    }
                                }
                                else
                                {
                                    strings.Add(new String(blob, cur, len - cur));
                                }
                                cur = nextNull + 1;
                            }

                            data = new String[strings.Count];
                            strings.CopyTo((String[])data, 0);
                        }
                        break;
                }
            }
            return data;
        }
        finally
        {
            if (regKeyHandle != IntPtr.Zero)
            {
                RegCloseKey(regKeyHandle);
            }
        }
    }
    public void WriteValue(string subKeyName, string valueName, object value, RegistryValueKind valueKind)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value can't be null");
        }
        if (valueName != null && valueName.Length > MaxValueLength)
        {
            throw new ArgumentException("value name is invalid");
        }

        IntPtr regKeyHandle = IntPtr.Zero;
        int ret = 0;
        try
        {
            if (RegOpenKeyEx(RootKey, subKeyName, 0, KEY_WRITE | WOWOption, out regKeyHandle) != 0)
            {
                throw new Exception("Failed to open registry key");
            }

            switch (valueKind)
            {
                case RegistryValueKind.ExpandString:
                case RegistryValueKind.String:
                    {
                        String data = value.ToString();
                        ret = RegSetValueEx(regKeyHandle, valueName, 0, valueKind, data, checked(data.Length * 2 + 2));
                        break;
                    }
                case RegistryValueKind.Binary:
                    byte[] dataBytes = (byte[])value;
                    ret = RegSetValueEx(regKeyHandle, valueName, 0, RegistryValueKind.Binary, dataBytes, dataBytes.Length);
                    break;

                case RegistryValueKind.DWord:
                    {
                        int data = Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
                        ret = RegSetValueEx(regKeyHandle, valueName, 0, RegistryValueKind.DWord, ref data, 4);
                        break;
                    }

                case RegistryValueKind.QWord:
                    {
                        long data = Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
                        ret = RegSetValueEx(regKeyHandle, valueName, 0, RegistryValueKind.QWord, ref data, 8);
                        break;
                    }
                default:
                    throw new NotImplementedException(string.Format("RegistryKind {0} not supported", valueKind));
            }

        }
        catch (Exception ex)
        {
            throw new Exception("Failed to write value", ex);
        }
        finally
        {
            if (regKeyHandle != IntPtr.Zero)
            {
                RegCloseKey(regKeyHandle);
            }
        }
    }
    public RegistryValueKind GetRegistryValueKind(string subKeyName, string valueName)
    {
        IntPtr regKeyHandle = IntPtr.Zero;
        try
        {
            if (RegOpenKeyEx(RootKey, subKeyName, 0, KEY_READ | WOWOption, out regKeyHandle) != 0)
            {
                throw new Exception("Failed to open registry key");
            }
            int type = 0;
            int datasize = 0;
            int ret = RegQueryValueEx(regKeyHandle, valueName, null, ref type, (byte[])null, ref datasize);
            if (ret != 0)
                throw new Win32Exception(ret);
            if (!Enum.IsDefined(typeof(RegistryValueKind), type))
            {
                return RegistryValueKind.Unknown;
            }
            return (RegistryValueKind)type;
        }
        finally
        {
            if (regKeyHandle != IntPtr.Zero)
            {
                RegCloseKey(regKeyHandle);
            }
        }
    }
}
