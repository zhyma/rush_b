using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

using System.IO.Ports;


public class SerialCom : MonoBehaviour
{
	// port name example: "COM6" or "\\\\.\\COM6"
	public string portName = "COM4";
	public int baudRate = 115200;
	private static SerialPort comPort = null;

	// message length
	// protocol: 0xFA, 0x00 (keys' state), 0x00 (distance, high), 0x00 (ditance low), 0xAF
	List<byte> buffer = new List<byte>();
	int messageLen = 5;

	// the states of all four switches.
	public static byte switchStates = 0;
	// the distance measured by TOF sensor, in "mm".
	public static int TOFDistance = BoltLogic.TOFLowThreshold;

	// ready to fire?
	// valve and electromagnet is not operating: false
	// either valve or electromagnet is operating: true
	public static bool operating = false;

	void Start()
	{
		//getPortName = "COM6";
		comPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
		comPort.ReadTimeout = 10;
		try
		{
			if (!comPort.IsOpen)
			{
				comPort.Open();
			}
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
		}
		StartCoroutine(CheckReceive());
	}

	IEnumerator CheckReceive()
	{
		while (true)
		{
			if (comPort != null && comPort.IsOpen)
			{
				try
				{
					int n = comPort.BytesToRead;
					byte[] buf = new byte[n];
					comPort.Read(buf, 0, n);
					buffer.AddRange(buf);
					while (buffer.Count >= 5)
					{
						// message start with 0xFA
						if (buffer[0] == 0xFA && buffer[4] == 0xAF)
						{
							byte[] processingByteArray = new byte[messageLen];
							buffer.CopyTo(0, processingByteArray, 0, messageLen);
							// Process the data
							DataProcess(processingByteArray);
							// remove processed data from the buffer
							buffer.RemoveRange(0, messageLen);
						}
						else
						{
							//corrupted message, remove the first one
							buffer.RemoveAt(0);
						}
					}
				}
				catch (Exception ex)
				{
					Debug.LogError(ex);
				}
			}
			yield return new WaitForSeconds(Time.deltaTime);
		}
	}

	// data processing
	private void DataProcess(byte[] dataBytes)
	{
        //Debug.Log(ByteShownAsHex(dataBytes));
		switchStates = dataBytes[3];
		// Receiving operating done flag from ESP32
		if ((switchStates & (1 << 4)) > 0)
			operating = false;
        TOFDistance = dataBytes[2];
	}

	// for debugging
	public static string ByteShownAsHex(byte[] bytes)
	{
		string hexStr = "";
		if (bytes != null)
		{
			for (int i = 0; i < bytes.Length; i++)
			{
				hexStr += bytes[i].ToString("X2") + ",";
			}
		}
		return hexStr;
	}

	public static void SendData(byte[] data)
	{
		if (comPort.IsOpen)
		{
			comPort.Write(data, 0, 1);
			operating = true;
		}
	}

	public void ClosePort()
	{
		try
		{
			comPort.Close();
		}
		catch (Exception ex)
		{
			Debug.Log(ex.Message);
		}
	}

	private void OnApplicationQuit()
	{
		ClosePort();
	}
	private void OnDisable()
	{
		ClosePort();
	}

}
