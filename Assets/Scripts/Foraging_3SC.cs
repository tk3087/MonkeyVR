using System.Collections;
using System.Collections.Generic;
using System; //Math
using System.IO; //Directory
using UnityEngine;




namespace Foraging3Screens
{
    
    
    
    
    public class FG3Screens
    {

		//-- Screen
		public float[,] CLUT = new float[256, 3];
		public double[,] dkl2rgb = new double[3, 3] { { 1, 1, 0.2991 }, { 1, -0.3274, -0.2711 }, { 1, 0.0166, 1 } };

		public uint screenSize_upperX, screenSize_upperY, screenSize_lowerX, screenSize_lowerY;

		public double pixSize = 19.7 / 1025;
		public double viewingDistCm = 57.3;
		public uint stereoMode = 0;
		public uint multiSamples = 8;
		public uint fps = 60;

		public double ifi;
		public double bg = 0.5;


		//-- End of Screen 

		//- setup

		public const int LEN_stimConcentration=2;
		public const int SIZE_1_stimTau=2;
		public const int VAR_nTrials = LEN_stimConcentration * SIZE_1_stimTau;


		double[,] fixationColor = new double[2,3]  { { 0,0,0 }, { 1,1,1 } };

		double fixationSizeDeg;
		double fixationThicknessDeg;
		double fixationTextDistDeg;

		double stimLambdaStart;                                         // Starting lambda value(prob of telegraph process)
		double[,]  stimTau = new double [2,3]  { {2,4,6},{4,8,12} };                    // Mean reward time in sec of each boxes, will be randomized
		double stimRepl;                                                // Repletion speed, tine constant of the exponential in sec
		double stimDepl;                                                // How much a box is depleted at each click(ratio)
		double stimReward;                                              // Points for correct click
		double stimCost;                                                // penalty for errors TOMKIL maybe int?
		double stimWidthDeg;                                            // Size of the stimulus fields in deg
		double stimDistDeg;                                             // Distance of the fields in deg
		double stimRange;                                               // Range of colors(keep 0.25 for blue to red)
																		//double[] stimConcentration = new double[2] { 0.06, 0.01 };                   // Concentration of the stimulus
																		//double[] stimConcentration = new double[2] { 0.06, 0.01 };                   // Concentration of the stimulus
		public double stimKappa;                                               // temporal weighting

		double cursorType;                                              // 1 = joystick, 2 = mouse
		double cursorSizeDeg;
		byte[,] cursorColor = new byte[3,3]  { {255,255,255},{0,255,0},{255,0,0} };
		double cursorSpeedDegSec;

		double durFixSec;
		double durFrameSec;
		double durTrialSec;
		double durItiSec;
		double durPauseSec;



		double fixationSizeRad;
		double fixationThicknessRad;
		double fixationTextDistRad;
		double stimWidthRad;
		double stimDistRad;
		double cursorSizeRad;
		double cursorSpeedRadSec;



		double fixationSizeCm;
		double fixationThicknessCm;
		double fixationTextDistCm;
		double stimWidthCm;
		double stimDistCm;
		double cursorSizeCm;
		double cursorSpeedCmSec;

		short nTrials = LEN_stimConcentration * SIZE_1_stimTau;

		double fixationSizePix;
		double fixationThicknessPix;
		double fixationTextDistPix;
		double stimWidthPix;
		double stimDistPix;
		double cursorSizePix;
		double cursorSpeedPixSec;

		double cursorSpeedPixFrames;

		public double[,] concentrationList = new double [3,VAR_nTrials];
		public double[,] tauList = new double [3,VAR_nTrials];

		//-- End of Setup

		//-- clut

		public const int VAR_npoints = 1000;
		

		double delta = 6 / 29; //fixed param pf cieluv
							   // lav and lva are non linear transf of xyz 

		double[] fitparam = { 0.6748, 21.8418, 9.1180 };


		
		// Measured xyz
		double rx, ry, rz; //% xyz of rgb
		double gx, gy, gz;
		double bx, by, bz;

		//% luminance of midpoint,it's use to do the transformation between spaces (gray half of max luminace)
		double[] wXYZ = { 37.3279, 36.9788, 69.0775 };

		// %% Lab to XYZ
		//	fLab2xyz = @(t)(t. ^ 3).*(t > delta. ^ 3) + ((t - 4. / 29).*3. * delta. ^ 2).*(t <= delta. ^ 3);
		// Yconv = @(L, Yr) Yr.*fLab2xyz((L + 16). / 116);
		// Xconv = @(L, a, Xr) Xr.*fLab2xyz(+a. / 500 + (L + 16). / 116);
		// Zconv = @(L, b, Zr) Zr.*fLab2xyz(-b. / 200 + (L + 16). / 116);
		// XYZconv = @(Lab, refXYZ)[Xconv(Lab(:, 1), Lab(:, 2), refXYZ(:, 1)), Yconv(Lab(:, 1), refXYZ(:, 2)), Zconv(Lab(:, 1), Lab(:, 3), refXYZ(:, 3))];

		// XYZ to RGB

		// RGB XYZ

		double[] rxyz, gxyz, bxyz;
		

		double[] rXYZ, gXYZ, bXYZ;
		


		double[,] M1, M2, invM1, invM2;
		double[] S;

		




		uint npoints, colsat, collum; // check implementation for details;

		double[] anglist;
		
		double[,] Labmat, XYZmat, RGBmat, repMat;
		





		public uint clutpoints;
		public uint clutpointsZIA;
		//for zero b indexed arrays
		public byte[,] Xrgb = new byte[VAR_npoints, 3];

		//End of clut 

		//trial 

		// COMPLEX of FFTW

		//End of trial

		public void screen()
        {




			



			string cwd, fname;

			// Not needed prob.
			screenSize_upperX = 0;
			screenSize_upperY = 0;
			screenSize_lowerX = 800;
			screenSize_lowerY = 800;
			// Not 
			
			ifi = 1 / fps;

			cwd = Directory.GetCurrentDirectory();
			//Debug.Log("Current path is:  " + cwd);
			fname = cwd + @"\Assets\ForagingParams\CLUT.csv";

			try
			{
				using (StreamReader sr = new StreamReader(fname))
				{
					string line;
					string []values = new string[3];

					int k = 0;
					// Read and display lines from the file until the end of
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						values = line.Split(',');

						for (int i = 0; i < 3; i++)
							CLUT[k, i] = float.Parse(values[i]);
							
						k++;
					}
					Debug.Log("CLUT loaded. Length=" + k.ToString());
				}
			}
			catch (Exception e)
			{
				// Let the user know what went wrong.
				Debug.Log("File: "+fname+" could not be read!");
				Debug.Log(e.Message);
			}


		}

		public void setup()
        {
			// fixationColor = { {0, 0, 0},{1, 1, 1} }; Initialized in class header file to preserve 
			fixationSizeDeg = 0.4;
			fixationThicknessDeg = 0.1;
			fixationTextDistDeg = 8;

			stimLambdaStart = 1.0;
			// stimTau = [2 4 6; 4 8 12];% Initialized in class header file to preserve 
			stimRepl = 0;
			stimDepl = 1;
			stimReward = +2;
			stimCost = -1;
			stimWidthDeg = 4;
			stimDistDeg = 8 / Math.Sqrt(3);
			stimRange = 0.25;
			//double[] stimConcentration = [0.06 0.01]; % Initialized in class header file to preserve 
			double[] stimConcentration = new double[2] { 0.2, 0.2 };                   // Concentration of the stimulus
			stimKappa = 0.5;

			cursorType = 1;                                 // 1 = joystick, 2 = mouse
			cursorSizeDeg = 0.4;
			// cursorColor = [255, 255, 255; 0, 255, 0; 255, 0, 0];
			cursorSpeedDegSec = 2;


			durFixSec = 0.5;
			durFrameSec = 2 / 60.0;
			durTrialSec = 5 * 60.0;
			durItiSec = 0.5;
			durPauseSec = 5;

			//convert units

			fixationSizeRad = fixationSizeDeg* Math.PI / 180;
			fixationThicknessRad = fixationThicknessDeg* Math.PI / 180;
			fixationTextDistRad = fixationTextDistDeg* Math.PI / 180;
			stimWidthRad = stimWidthDeg* Math.PI / 180;
			stimDistRad = stimDistDeg* Math.PI / 180;
			cursorSizeRad = cursorSizeDeg* Math.PI / 180;
			cursorSpeedRadSec = cursorSpeedDegSec* Math.PI / 180;


			fixationSizeCm = viewingDistCm* Math.Tan(fixationSizeRad);
			fixationThicknessCm = viewingDistCm* Math.Tan(fixationThicknessRad);
			fixationTextDistCm = viewingDistCm* Math.Tan(fixationTextDistRad);
			stimWidthCm = viewingDistCm* Math.Tan(stimWidthRad);
			stimDistCm = viewingDistCm* Math.Tan(stimDistRad);
			cursorSizeCm = viewingDistCm* Math.Tan(cursorSizeRad);
			cursorSpeedCmSec = viewingDistCm* Math.Tan(cursorSpeedRadSec);

			fixationSizePix = fixationSizeCm / pixSize;
			fixationThicknessPix = fixationThicknessCm / pixSize;
			fixationTextDistPix = fixationTextDistCm / pixSize;
			stimWidthPix = Math.Round(stimWidthCm / pixSize);
			stimDistPix = stimDistCm / pixSize;
			cursorSizePix = cursorSizeCm / pixSize;
			cursorSpeedPixSec = cursorSpeedCmSec / pixSize;

			cursorSpeedPixFrames = cursorSpeedPixSec* durFrameSec;



			// generate experiment structure


			nTrials = LEN_stimConcentration* SIZE_1_stimTau;
			// nTrials = length(stimConcentration) * size(stimTau, 1);


			//concentrationList = NaN(3, nTrials);
			//tauList = NaN(3, nTrials);

			int trial = 0;

			System.Random rand = new System.Random(); 

			for (int i = 0; i<LEN_stimConcentration; i++)
				for (int j = 0; j<SIZE_1_stimTau; j++)
				{
					for (int k = 0; k< 3; k++)
					{

						concentrationList[k,trial] = stimConcentration[i];
						tauList[k,trial] = stimTau[i,j];
						
					}
					trial++;

				}

			int[] randTrails = new int[VAR_nTrials];

			// set the Vector 1..N
			for (int i = 0; i < VAR_nTrials; i++)
				randTrails[i] = i;

			// Random permutation 
			for (int i = 0; i < VAR_nTrials; i++)
			{
				int j, k;
				j = rand.Next() % (VAR_nTrials - i) + i;
				k = randTrails[j]; randTrails[j] = randTrails[i]; randTrails[i] = k; // Swap i and j
			}
			double[,] perm_concentrationList = new double[3,VAR_nTrials];
			double[,] perm_tauList = new double[3,VAR_nTrials];

			for (int t = 0; t < VAR_nTrials; t++)

				for (int i = 0; i < 3; i++)
				{
					perm_concentrationList[i,t] = concentrationList[i,randTrails[t]];
					perm_tauList[i,t] = tauList[i,randTrails[t]];

				}

			//copy perm matrix to original
			for (int t = 0; t < VAR_nTrials; t++)
				for (int i = 0; i < 3; i++)
				{
					concentrationList[i,t] = perm_concentrationList[i,t];
					tauList[i,t] = perm_tauList[i,t];

				}

        }
		public void clut()
        {
			
			npoints = VAR_npoints;
			
			rxyz = new double[3]; gxyz = new double[3]; bxyz = new double[3];
			rXYZ = new double[3]; gXYZ = new double[3]; bXYZ = new double[3];
			
			M1 = new double[3, 3]; M2 = new double[3, 3]; 
			invM1 = new double[3, 3]; invM2 = new double[3, 3];
			S = new double[3];

			anglist = new double[VAR_npoints];

			Labmat = new double[VAR_npoints, 3]; XYZmat = new double[VAR_npoints, 3]; RGBmat = new double[VAR_npoints, 3]; repMat = new double[VAR_npoints, 3];


			// Measured xyz
			rx = 0.5273; ry = 0.2997; rz = 0.1729; //% xyz of rgb
			gx = 0.3233; gy = 0.5959; gz = 0.0809;
			bx = 0.1521; by = 0.0694; bz = 0.7786;


			rxyz[0] = rx; rxyz[1] = ry; rxyz[2] = rz;
			gxyz[0] = gx; gxyz[1] = gy; gxyz[2] = gz;
			bxyz[0] = bx; bxyz[1] = by; bxyz[2] = bz;

			double wXYZ_Y = wXYZ[1];
			// normalized to y
			for (int i = 0; i < 3; i++)
			{


				rXYZ[i] = rxyz[i] / ry;
				gXYZ[i] = gxyz[i] / gy;
				bXYZ[i] = bxyz[i] / by;

				//% reference point - midpoint of monitior(xyz)

				wXYZ[i] = wXYZ[i] / wXYZ_Y;


			}

			for (int i = 0; i < 3; i++)
			{
				M1[i,0] = rXYZ[i];
				M1[i,1] = gXYZ[i];
				M1[i,2] = bXYZ[i];
			}

			inv3x3(M1, invM1);

			mult3x1(invM1, wXYZ, S);

			double[,] diagS;
			diagS = new double [3,3];

			diag_3(S, diagS);

			mult3x3(M1, diagS, M2);

			inv3x3(M2, invM2);
			//
			//  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
			//  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
			//  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
			//  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
			//
			//
			// Build equisaturation circle
			colsat = 50;    //25 saturation (ratio max gamut for this monitor?
			collum = 100;   // 100fraction luminance at 45 deg viewing angle

			double angle, ang_step = 2 * Math.PI / (npoints - 1);
			angle = 0;

			for (int i = 0; i < npoints; i++)
			{
				//anglist[i] = angle;

				Labmat[i,0] = collum;
				Labmat[i,1] = colsat * Math.Cos(angle);
				Labmat[i,2] = colsat * Math.Sin(angle);

				angle += ang_step;
			}



			repmatscale((1 - fitparam[0]), wXYZ, VAR_npoints, repMat);

			XYZconv(Labmat, repMat, XYZmat);

			mult3x1000transp(invM2, XYZmat, RGBmat);




			clutpoints = npoints;
			clutpointsZIA = clutpoints - 1;

			//
			//  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
			//  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
			//  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
			//  >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>><<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
			//
			//


			double gammaCorrection = 2;
			for (int i = 0; i < VAR_npoints; i++)
				for (int j = 0; j < 3; j++)
					//Xrgb[i,j] = (byte)(255 * RGBmat[i,j]);
					Xrgb[i, j] = (byte)(255 * Math.Pow(RGBmat[i, j],1/gammaCorrection));
					//Xrgb[i,j] = (byte)(255 * (RGBmat[i,j]+0.2));


		}
		void diag_3(double [] mat, double[,] diag_mat)
        {

			int i, j;

			for (i = 0; i < 3; i++)
				for (j = 0; j < 3; j++)
					diag_mat[i,j] = (double)0;
			for (i = 0; i < 3; i++)
				diag_mat[i,i] = mat[i];

		}
		
		void mult3x1(double[,] mat_a, double[] mat_b, double[] mat_r)
		{
			int i, j, k;
			double s = 0;

			for (i = 0; i < 3; i++)
			{
				for (k = 0; k < 3; k++)
					s += mat_a[i,k] * mat_b[k];

				mat_r[i] = s;
				s = 0;
			}



		}

		double fLab2xyz(double t)
		{
			double delta_cubed, res;

			delta_cubed = delta * delta * delta;

			if (t > delta_cubed)
				res = t * t * t;
			else
				res = 3.0 * (t - 4 / 29) * delta * delta;

			return res;
		}

		double Yconv(double L, double Yr)
		{
			return Yr * fLab2xyz((L + 16) / 116);

		}

		double Xconv(double L, double a, double Xr)
		{
			return Xr * fLab2xyz(a / 500 + (L + 16) / 116);
		}

		double Zconv(double L, double b, double Zr)
		{
			return Zr * fLab2xyz(-b / 200 + (L + 16) / 116);
		}

		void repmatscale(double scale, double[] vec, int rows, double[,] mat_r)
		{

			int i, j;

			for (i = 0; i < rows; i++)
				for (j = 0; j < 3; j++)
					mat_r[i,j] = scale * vec[j];



		}

		void XYZconv(double[,] Lab, double[,] refXYZ, double[,] XYZmat)
		{
			int i;


			for ( i = 0; i < VAR_npoints; i++)
			{
				XYZmat[i,0] = Xconv(Lab[i,0], Lab[i,1], refXYZ[i,0]);
				XYZmat[i,1] = Yconv(Lab[i,0], refXYZ[i,1]);
				XYZmat[i,2] = Zconv(Lab[i,0], Lab[i,2], refXYZ[i,2]);


			}
		}

		void mult3x3(double[,] mat_a, double[,] mat_b, double[,] mat_r)
		{
			int i, j, k;
			double s = 0;

			for (i = 0; i < 3; i++)
				for (j = 0; j < 3; j++)
				{
					for (k = 0; k < 3; k++)
						s += mat_a[i,k] * mat_b[j,k];

					mat_r[i,j] = s;
					s = 0;
				}


		}

		void inv3x3(double[,] mat, double[,] inv_mat)
		{
			double determinant = 0;
			int i, j;

			//finding determinant
			for (i = 0; i < 3; i++)
				determinant = determinant + (mat[0,i] * (mat[1,(i + 1) % 3] * mat[2,(i + 2) % 3] - mat[1,(i + 2) % 3] * mat[2,(i + 1) % 3]));

			for (i = 0; i < 3; i++)
			{
				for (j = 0; j < 3; j++)
					inv_mat[i,j] = ((mat[(j + 1) % 3,(i + 1) % 3] * mat[(j + 2) % 3,(i + 2) % 3]) - (mat[(j + 1) % 3,(i + 2) % 3] * mat[(j + 2) % 3,(i + 1) % 3])) / determinant;


			}
		}


		void mult3x1000transp(double[,] mat_a, double[,] mat_b, double[,] mat_r)
		{

			// ATTN 3x3 by 3x1000 (< Transp input) =  result 3x1000 transposed ATTN

			int i, j, k;
			double s = 0;

			for (i = 0; i < VAR_npoints; i++)
				for (j = 0; j < 3; j++)
				{
					for (k = 0; k < 3; k++)
						s += mat_a[j,k] * mat_b[i,k];

					mat_r[i,j] = s;
					s = 0;
				}




		}












	}




}