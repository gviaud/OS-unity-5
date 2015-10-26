using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

	class PdfToPng
	{
        [DllImport("gsdll32", EntryPoint = "gsapi_new_instance")]
        private static extern int CreateAPIInstance(out IntPtr pinstance,
                                                IntPtr caller_handle);

        [DllImport("gsdll32", EntryPoint = "gsapi_init_with_args")]
        private static extern int InitAPI(IntPtr instance, int argc, IntPtr argv);

        [DllImport("gsdll32", EntryPoint = "gsapi_exit")]
        private static extern int ExitAPI(IntPtr instance);

        [DllImport("gsdll32", EntryPoint = "gsapi_delete_instance")]
        private static extern void DeleteAPIInstance(IntPtr instance);





        public void DoTheJob(string inputPath, string outputPath,
                              int firstPage, int lastPage, int width, int height)
        {
            CallAPI(GetArgs(inputPath, outputPath, firstPage, lastPage, width, height));
        }

        private void CallAPI(string[] args)
        {
            var argStrHandles = new GCHandle[args.Length];
            var argPtrs = new IntPtr[args.Length];

            // Create a handle for each of the arguments after 
            // they've been converted to an ANSI null terminated
            // string. Then store the pointers for each of the handles
            for (int i = 0; i < args.Length; i++)
            {
                argStrHandles[i] = GCHandle.Alloc(StringToAnsi(args[i]), GCHandleType.Pinned);
                argPtrs[i] = argStrHandles[i].AddrOfPinnedObject();
            }

            // Get a new handle for the array of argument pointers
            var argPtrsHandle = GCHandle.Alloc(argPtrs, GCHandleType.Pinned);

            // Get a pointer to an instance of the GhostScript API 
            // and run the API with the current arguments
            IntPtr gsInstancePtr;
            CreateAPIInstance(out gsInstancePtr, IntPtr.Zero);
            InitAPI(gsInstancePtr, args.Length, argPtrsHandle.AddrOfPinnedObject());
            Cleanup(argStrHandles, argPtrsHandle, gsInstancePtr);
        }

        private string[] GetArgs(string inputPath, string outputPath,
                         int firstPage, int lastPage, int width, int height)
        {
            return new[]
            {
                // Keep gs from writing information to standard output
                "-q",                     
                "-dQUIET",
       
                "-dPARANOIDSAFER", // Run this command in safe mode
                "-dBATCH", // Keep gs from going into interactive mode
                "-dNOPAUSE", // Do not prompt and pause for each page
                "-dNOPROMPT", // Disable prompts for user interaction           
                "-dMaxBitmap=500000000", // Set high for better performance
        
                // Set the starting and ending pages
                String.Format("-dFirstPage={0}", firstPage),
                String.Format("-dLastPage={0}", lastPage),   
        
                // Configure the output anti-aliasing, resolution, etc
                "-dAlignToPixels=0",
                "-dGridFitTT=0",
                "-sDEVICE=jpeg",
                "-dTextAlphaBits=4",
                "-dGraphicsAlphaBits=4",
                String.Format("-r{0}x{1}", width, height),

                // Set the input and output files
                String.Format("-sOutputFile={0}", outputPath),
                inputPath
            };
        }

        public static byte[] StringToAnsi(string original)
        {
               var strBytes = new byte[original.Length + 1];
               for (int i = 0; i < original.Length; i++)
                    strBytes[i] = (byte)original[i];
        
                strBytes[original.Length] = 0;
                return strBytes;
        }

        private void Cleanup(GCHandle[] argStrHandles, GCHandle argPtrsHandle,
                                       IntPtr gsInstancePtr)
        {
            for (int i = 0; i < argStrHandles.Length; i++)
                argStrHandles[i].Free();

            argPtrsHandle.Free();
            ExitAPI(gsInstancePtr);
            DeleteAPIInstance(gsInstancePtr);
        }

	}
