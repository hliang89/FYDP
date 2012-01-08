using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FYDP_DataStructure
{
    public class SerialPort_Input
    {
        public double SP_AccelX { get; set; }
        public double SP_AccelY { get; set; }
        public double SP_AccelZ { get; set; }
        public double SP_GyroX { get; set; }
        public double SP_GyroY { get; set; }
        public double SP_GyroZ { get; set; }
        public double SP_MagnetoX { get; set; }
        public double SP_MagentoY { get; set; }
        public double SP_MagnetoZ { get; set; }

        public object lockThis;

        public SerialPort_Input(string readLine)
        {
            lockThis = new object();

            string[] inputSplit = readLine.Split(',');

            lock (lockThis)
            {
                try
                {
                    SP_AccelX = (Convert.ToInt16(inputSplit[0].Substring(0), 16) / (Math.Pow(2, 15))) * 8 * 9.81;
                    SP_AccelY = (Convert.ToInt16(inputSplit[1].Substring(0), 16) / (Math.Pow(2, 15))) * 8 * 9.81;
                    SP_AccelZ = (Convert.ToInt16(inputSplit[2].Substring(0), 16) / (Math.Pow(2, 15))) * 8 * 9.81;
                    SP_GyroX = (Convert.ToInt16(inputSplit[6].Substring(0), 16) / (Math.Pow(2, 15))) * 500 / 180 * Math.PI;
                    SP_GyroY = (Convert.ToInt16(inputSplit[7].Substring(0), 16) / (Math.Pow(2, 15))) * 500 / 180 * Math.PI;
                    SP_GyroZ = ((Convert.ToInt16(inputSplit[8].Substring(0, 4), 16) / (Math.Pow(2, 15))) * 500 / 180 * Math.PI);
                    SP_MagnetoX = (Convert.ToInt16(inputSplit[3].Substring(0), 16) / (Math.Pow(2, 15))) * 8;
                    SP_MagentoY = (Convert.ToInt16(inputSplit[4].Substring(0), 16) / (Math.Pow(2, 15))) * 8;
                    SP_MagnetoZ = (Convert.ToInt16(inputSplit[5].Substring(0), 16) / (Math.Pow(2, 15))) * 8;
                }
                catch
                {
                }
            }
        }



        public void DisplayParsedInput()
        {
            Console.WriteLine("accel x: {0},accel_y: {1}, accel_z: {2}", SP_AccelX,
                            SP_AccelY, SP_AccelZ);
            Console.WriteLine("gyro x: {0}, gyro_y: {1}, gyro_z: {2}", SP_GyroX,
                SP_GyroY, SP_GyroZ);
            Console.WriteLine("mag x: {0}, mag_y: {1}, mag_z: {2}", SP_MagnetoX,
                SP_MagentoY, SP_MagnetoZ);
        }


    }
}
