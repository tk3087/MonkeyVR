using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using System.Diagnostics;
using FFTW.NET;


public class nativeCode : MonoBehaviour
{
	// Start is called before the first frame update
	int count = 0;
	
	void Start()
    {
		
		UnityEngine.Debug.Log("Started!\n");
			
		Stopwatch stopwatch = new Stopwatch();
		//stopwatch.Start();
		using (var input = new AlignedArrayComplex(16, 2048, 2048))
		using (var output = new AlignedArrayComplex(16, input.GetSize()))
		{
			UnityEngine.Debug.Log("Row = "+ input.GetLength(0).ToString() +"  Col= " + input.GetLength(1).ToString() + "\n");
			for (int row = 0; row < input.GetLength(0); row++)
			{
				for (int col = 0; col < input.GetLength(1); col++)
					input[row, col] = (double)row * col / input.Length;
			}

			DFT.FFT(input, output);
			count++;
			stopwatch.Start();
			DFT.IFFT(output, output);
			stopwatch.Stop();
			/*
			for (int row = 0; row < input.GetLength(0); row++)
			{
				UnityEngine.Debug.Log("Row----------------------------------> " + row.ToString() + "\n");
				for (int col = 0; col < input.GetLength(1); col++)
					//Console.Write((output[row, col].Real / input[row, col].Real / input.Length).ToString("F2").PadLeft(6));
					UnityEngine.Debug.Log((output[row, col].Real / input[row, col].Real / input.Length).ToString("F2").PadLeft(6));
				//Console.WriteLine();
				UnityEngine.Debug.Log("/n");

			}
			*/
		}
		
		UnityEngine.Debug.Log("Time Elapsed of IFFT2D= "+ stopwatch.ElapsedMilliseconds.ToString() +"\n" );
		UnityEngine.Debug.Log("Finished! [" + count.ToString()+"]\n");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
