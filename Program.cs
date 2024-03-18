using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace PCFingerprint {
   class Program {
      static void Main (string[] args) {
         string fingerprint = GetPCFingerprint ();
         Console.WriteLine ("PC Fingerprint: " + fingerprint);
      }

      static string GetPCFingerprint () {
         StringBuilder fingerprintBuilder = new StringBuilder ();

         var cpuID = GetCPUId ();
         var motherboardSerial = GetMotherboardSerialNumber ();
         var diskDriveSerial = GetDiskDriveSerialNumber ();
         var diskDrive2Serial = GetDiskDriveSerialNumber2 ();
         var osVersion = Environment.OSVersion.ToString ();
         var machineName = Environment.MachineName;
         Console.WriteLine (
            $"CPU ID: {cpuID}\n" +
            $"MotherBoard Serial: {motherboardSerial}\n" +
            $"DiskDrive Serial: {diskDriveSerial}\n" +
            $"DiskDrive2 Serial: {diskDrive2Serial}\n" +
            $"osVersion: {osVersion}\n" +
            $"Machine Name: {machineName}\n"
            );

         // Gather hardware identifiers
         fingerprintBuilder.Append (cpuID);
         fingerprintBuilder.Append (motherboardSerial);
         fingerprintBuilder.Append (diskDriveSerial);

         // Concatenate additional system information
         fingerprintBuilder.Append (osVersion);
         fingerprintBuilder.Append (machineName);

         // Hash the fingerprint
         using (SHA256 sha256 = SHA256.Create ()) {
            byte[] hashBytes = sha256.ComputeHash (Encoding.UTF8.GetBytes (fingerprintBuilder.ToString ()));
            return BitConverter.ToString (hashBytes).Replace ("-", "").ToLower ();
         }
      }

      static string GetCPUId () {
         // Retrieve CPU ID using ManagementObject
         ManagementObjectSearcher searcher = new ManagementObjectSearcher ("SELECT ProcessorId FROM Win32_Processor");
         foreach (ManagementObject obj in searcher.Get ()) {
            return obj["ProcessorId"].ToString ();
         }
         return string.Empty;
      }

      static string GetMotherboardSerialNumber () {
         // Retrieve motherboard serial number using ManagementObject
         ManagementObjectSearcher searcher = new ManagementObjectSearcher ("SELECT SerialNumber FROM Win32_BaseBoard");
         foreach (ManagementObject obj in searcher.Get ()) {
            return obj["SerialNumber"].ToString ();
         }
         return string.Empty;
      }

      static string GetDiskDriveSerialNumber () {
         // Retrieve disk drive serial number using ManagementObject
         ManagementObjectSearcher searcher = new ("SELECT SerialNumber FROM Win32_DiskDrive WHERE MediaType='Fixed hard disk media'");
         foreach (ManagementObject obj in searcher.Get ()) {
            return obj["SerialNumber"].ToString ();
         }
         return string.Empty;
      }

      static string GetDiskDriveSerialNumber2() {
         try {
            // Query WMI for disk drive information
            ManagementObjectSearcher searcher = new ManagementObjectSearcher ("SELECT SerialNumber FROM Win32_DiskDrive");
            ManagementObjectCollection collection = searcher.Get ();

            foreach (ManagementObject obj in collection) {
               // Return the serial number of the first disk drive found
               return obj["SerialNumber"].ToString ();
            }

            // No disk drive found
            Console.WriteLine ("No disk drive found.");
            return null;
         } catch (Exception ex) {
            // Handle any exceptions
            Console.WriteLine ($"Error retrieving disk drive serial number: {ex.Message}");
            return null;
         }
      }
   }
}
