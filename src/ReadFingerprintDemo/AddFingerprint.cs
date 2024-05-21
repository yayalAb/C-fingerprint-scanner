using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using SourceAFIS.Simple;
using Futronic.Devices.FS26;
using System.Threading;
using System.IO;
using System.Drawing.Imaging;

namespace ReadFingerprintDemo
{
    public class AddFingerprint
    {
        string FingerPrint;
        public Bitmap[] AddedFingerPrints;
        private AfisEngine _afis;
        bool fingerpint = false;
        string massage = "Fingerprint Detetcted!";
        int statusCode = 200;
        public (string, string, int) ScannFIngerPrint(string userId, string Index, bool IsNewUser, bool CheckDuplication, bool idDesposRequest)
        {
            DisplayNotificationClass notificationObj = new DisplayNotificationClass();
            var device = new DeviceAccessor().AccessFingerprintDevice();

            if (idDesposRequest)
            {
                device.Dispose();
                this.IsNewUser();
                return (null, "Desposed", statusCode);
            }
            ManualResetEvent fingerprintDetectedEvent = new ManualResetEvent(false);
            Bitmap bitmapFingerprint = null;
            device.SwitchLedState(true, false);
            device.FingerDetected += (sender, args) =>
            {
                FingerPrint = HandleNewFingerprint(bitmapFingerprint = device.ReadFingerprint());
                fingerprintDetectedEvent.Set();
            };
            device.StartFingerDetection();
            notificationObj.displayNotfication("Please place your finger on the device or press enter to cancel");
            if (fingerprintDetectedEvent.WaitOne(10000))
            {
                if (!device.IsFingerPresent)
                {

                    notificationObj.displayNotfication("waiting .......");
                }
            }
            else
            {
                massage = "Connection time out";
                notificationObj.displayNotfication("Connection time out");
            }
            if (CheckDuplication)
            {
                notificationObj.displayNotfication("Validating  Fingerprint ..... ");

                if (bitmapFingerprint != null && (bitmapFingerprint is Bitmap))
                {
                    device.Dispose();
                    // await ValidateFingerprint(bitmapFingerprint, userId, Index, IsNewUser);
                }
            }
            // device.SwitchLedState(false, true);
            //  device.Dispose();

            return (FingerPrint, massage, statusCode);
        }

        private string HandleNewFingerprint(Bitmap bitmap)
        {
            byte[] imageData;
            string base64String = null;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png); // save the bitmap to a memory stream as a PNG image
                imageData = ms.ToArray(); // get the bytes from the memory stream
                base64String = Convert.ToBase64String(imageData);
            }
            massage = "Fingerprint registered";
            return base64String;

        }
        public async Task ValidateFingerprint(Bitmap bitmap, string Id, string index, bool isNewuser)
        {
            DisplayNotificationClass notificationObj = new DisplayNotificationClass();

            if (File.Exists("Tempdata/" + Id + index + ".bmp"))
            {
                File.Delete("Tempdata/" + Id + index + ".bmp");
            }

            if (isNewuser)
            {
                IsNewUser();
            }

            _afis = new AfisEngine();
            var allFingers = new List<Person>();
            var allBitmaps = Directory.GetFiles("Tempdata", "*.bmp", SearchOption.TopDirectoryOnly).Select(Path.GetFileName);
            int i = 0;

            if (allBitmaps.FirstOrDefault() != null)
            {
                // Parallelize the processing using Parallel.ForEach
                Parallel.ForEach(allBitmaps, bitmapFile =>
                {
                    var person = new Person();
                    person.Id = Interlocked.Increment(ref i);
                    var fingerprintId = Path.GetFileNameWithoutExtension(bitmapFile);
                    var patternFile = $"{fingerprintId}.min";

                    Bitmap bitmap1 = new Bitmap(Path.Combine("Tempdata", bitmapFile));
                    Fingerprint fp = new Fingerprint();
                    fp.AsBitmap = bitmap1;
                    person.Fingerprints.Add(fp);
                    // Extract the fingerprint in parallel
                    lock (_afis) // Lock the AfisEngine to ensure thread safety
                    {
                        _afis.Extract(person);
                    }

                    allFingers.Add(person);
                });
            }

            var newFinger = new Person();
            var fingerprint = new Fingerprint();
            fingerprint.AsBitmap = bitmap;
            newFinger.Fingerprints.Add(fingerprint);

            // Extract the new fingerprint
            _afis.Extract(newFinger);

            var matches = _afis.Identify(newFinger, allFingers);
            var persons = matches as Person[] ?? matches.ToArray();

            foreach (var person in persons)
            {
                var personId = person.Id;
                massage = $"Duplicate Finger Enrolled with index {personId}!";
                statusCode = 404;

                notificationObj.displayNotfication($"Duplicate Finger Enrolled with index {personId}!");

            }

            if (!persons.Any())
            {
                try
                {
                    string fileName = Id + index;
                    string randomFilename = fileName + ".bmp";

                    if (File.Exists("Tempdata/" + randomFilename))
                    {
                        File.Delete("Tempdata/" + randomFilename);
                    }

                    Console.WriteLine("trying to save file {0}", randomFilename);

                    if (!File.Exists("Tempdata/" + randomFilename))
                    {
                        bitmap.Save(Path.Combine("Tempdata", randomFilename));
                    }

                    massage = "Fingerprint Enrolled Successfully";
                    notificationObj.displayNotfication("Fingerprint Enrolled Successfully!");

                }
                catch (SystemException ex)
                {
                    throw new Exception("System Exception!");
                }
                catch (Exception ex)
                {
                    throw new Exception("Error on saving");
                }
            }
        }

        public void IsNewUser()
        {
            if (!Directory.Exists("Tempdata"))
            {
                Directory.CreateDirectory("Tempdata");
            }
            try
            {
                string[] files = Directory.GetFiles("Tempdata");
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        // The file is being used by another process, wait for a short period of time and try again
                        Thread.Sleep(500);
                        try
                        {
                            File.Delete(file);
                        }
                        catch (IOException ex)
                        {
                            // Handle the exception if the file is still being used after waiting
                            Console.WriteLine($"Failed to delete file: {ex.Message}");

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Empty file");
            }



        }
    }
}

