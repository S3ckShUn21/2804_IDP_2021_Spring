using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Input;
using WirelessSensorNodeDashboard.Commands;

namespace WirelessSensorNodeDashboard.ViewModels
{
    public sealed class MainUIViewModel : BaseViewModel
    {

        private ICommand _testCommand;

        public ICommand TestCommand
        {
            get => _testCommand;
            set => SetPropertyAndNotify(ref _testCommand, value, nameof(TestCommand));
        }

        public MainUIViewModel()
        {
            _testCommand = new RelayCommand(testFunction);
        }


        private void testFunction()
        {
            Debug.WriteLine("Test Function Start");

            // Reading Data

            string path = @"SavedData.csv";
            Debug.WriteLine(path);
            string[] lines = File.ReadAllLines(path);
            /*foreach( string line in lines )
            {
                Debug.WriteLine(line);
            }*/

            // Writing Data to a file and reading it back
            string writtenPath = @"WrittenData.csv";
            if (!File.Exists(writtenPath))
            {
                using (StreamWriter sw = File.CreateText(writtenPath))
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        DateTime time = DateTime.Now;
                        sw.WriteLine("{0},{1:D4}", time.ToString("yyyy-MM-dd HH:mm:ss:ff"), i);
                        Thread.Sleep(10);
                    }
                }
                Debug.WriteLine(writtenPath);
                lines = File.ReadAllLines(writtenPath);
                foreach (string line in lines)
                {
                    Debug.WriteLine(line);
                }
            }
            
            // Reading the last 30 lines of the csv file
            using (FileStream file = File.OpenRead(writtenPath))
            {
                // Read from the front of the file
                byte[] buffer = new byte[32];
                file.Read(buffer, 0, 32);
                for( int i = 0; i<32; i++)
                {
                    Debug.Write((char)buffer[i]);
                }
                Debug.WriteLine("");

                // Testing the reading from the back
                file.Seek(0, SeekOrigin.End);
                int newlineCount = 0;
                for( int i = 1; i <= 300; i++ )
                {
                    file.Seek(-i, SeekOrigin.End);
                    char val = (char)file.ReadByte();
                    if( val == '\n')
                    {
                        newlineCount++;
                    }
                    //Debug.WriteLine("{0} => {1}",val ,(char)val);
                }
                Debug.WriteLine(newlineCount);

                // Trying to read just the last 5 lines
                Debug.WriteLine("5 Lines Test");
                newlineCount = 0;
                long filePointer = 0;
                long fileSize = file.Length;
                while( newlineCount < 6 && filePointer < fileSize )
                {
                    file.Seek(-filePointer, SeekOrigin.End);
                    if((char)file.ReadByte() == '\n')
                    {
                        newlineCount++;
                    }
                    filePointer++;
                }
                byte[] endingBuffer = new byte[300];
                file.Read(endingBuffer, 0, 300);
                Debug.WriteLine(Encoding.Default.GetString(endingBuffer));

            }

            Debug.WriteLine("Test Function End");
        }


    }
}
