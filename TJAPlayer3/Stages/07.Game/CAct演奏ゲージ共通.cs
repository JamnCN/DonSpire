using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using FDK;

namespace TJAPlayer3
{
    /// <summary>
    /// CAct演奏Drumsゲージ と CAct演奏Gutiarゲージ のbaseクラス。ダメージ計算やDanger/Failed判断もこのクラスで行う。
    /// 
    /// 課題
    /// _STAGE FAILED OFF時にゲージ回復を止める
    /// _黒→閉店までの差を大きくする。
    /// </summary>
    internal class CAct演奏ゲージ共通 : CActivity
    {
        // プロパティ
        public CActLVLNFont actLVLNFont { get; protected set; }

        // コンストラクタ

        // CActivity 実装

        public override void On活性化()
        {
            for (int i = 0; i < 3; i++)
            {
                dbゲージ増加量[i] = new float[2];
                for (int n = 0; n < 3; n++)
                {
                    dbゲージ増加量_Branch[i, n] = new float[2];
                }
            }
            this.DTX[0] = TJAPlayer3.DTX;
            this.DTX[1] = TJAPlayer3.DTX_2P;
            actLVLNFont = new CActLVLNFont();
            actLVLNFont.On活性化();
            base.On活性化();
        }
        public override void On非活性化()
        {
            actLVLNFont.On非活性化();
            actLVLNFont = null;
            base.On非活性化();
        }

        const double GAUGE_MAX = 100.0;
        const double GAUGE_INITIAL = 2.0 / 3;
        const double GAUGE_MIN = -0.1;
        const double GAUGE_ZERO = 0.0;
        const double GAUGE_DANGER = 0.3;

        public bool bRisky                          // Riskyモードか否か
        {
            get;
            private set;
        }
        public int nRiskyTimes_Initial              // Risky初期値
        {
            get;
            private set;
        }
        public int nRiskyTimes                      // 残Miss回数
        {
            get;
            private set;
        }
        public bool IsFailed(E楽器パート part)   // 閉店状態になったかどうか
        {
            if (bRisky)
            {
                return (nRiskyTimes <= 0);
            }
            return this.db現在のゲージ値[(int)part] <= GAUGE_MIN;
        }
        public bool IsDanger(E楽器パート part)   // DANGERかどうか
        {
            if (bRisky)
            {
                switch (nRiskyTimes_Initial)
                {
                    case 1:
                        return false;
                    case 2:
                    case 3:
                        return (nRiskyTimes <= 1);
                    default:
                        return (nRiskyTimes <= 2);
                }
            }
            return (this.db現在のゲージ値[(int)part] <= GAUGE_DANGER);
        }

        /// <summary>
        /// ゲージの初期化
        /// </summary>
        /// <param name="nRiskyTimes_Initial_">Riskyの初期値(0でRisky未使用)</param>
        public void Init(int nRiskyTimes_InitialVal, int nPlayer)       // ゲージ初期化
        {
            //ダメージ値の計算は太鼓の達人譜面Wikiのものを参考にしました。

            for (int i = 0; i < 4; i++)
            {
                this.db現在のゲージ値[i] = 0;
            }

            //ゲージのMAXまでの最低コンボ数を計算
            float dbGaugeMaxComboValue = 0;
            float[] dbGaugeMaxComboValue_branch = new float[3];
            float dbDamageRate = 2.0f;

            if (nRiskyTimes_InitialVal > 0)
            {
                this.bRisky = true;
                this.nRiskyTimes = TJAPlayer3.ConfigIni.nRisky;
                this.nRiskyTimes_Initial = TJAPlayer3.ConfigIni.nRisky;
            }

            switch (this.DTX[nPlayer].LEVELtaiko[TJAPlayer3.stage選曲.n確定された曲の難易度[nPlayer]])
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    {
                        if (this.DTX[nPlayer].bチップがある.Branch)
                        {
                            dbGaugeMaxComboValue = this.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[0] / 100.0f);
                            for (int i = 0; i < 3; i++)
                            {
                                dbGaugeMaxComboValue_branch[i] = this.DTX[nPlayer].nノーツ数_Branch[i] * (this.fGaugeMaxRate[0] / 100.0f);
                            }
                            dbDamageRate = 0.625f;
                        }
                        else
                        {
                            dbGaugeMaxComboValue = this.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[0] / 100.0f);
                            dbDamageRate = 0.625f;
                        }
                        break;
                    }


                case 8:
                    {
                        if (this.DTX[nPlayer].bチップがある.Branch)
                        {
                            dbGaugeMaxComboValue = this.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[1] / 100.0f);
                            for (int i = 0; i < 3; i++)
                            {
                                dbGaugeMaxComboValue_branch[i] = this.DTX[nPlayer].nノーツ数_Branch[i] * (this.fGaugeMaxRate[1] / 100.0f);
                            }
                            dbDamageRate = 0.625f;
                        }
                        else
                        {
                            dbGaugeMaxComboValue = this.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[1] / 100.0f);
                            dbDamageRate = 0.625f;
                        }
                        break;
                    }

                case 9:
                case 10:
                    {
                        if (this.DTX[nPlayer].bチップがある.Branch)
                        {
                            dbGaugeMaxComboValue = this.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[2] / 100.0f);
                            for (int i = 0; i < 3; i++)
                            {
                                dbGaugeMaxComboValue_branch[i] = this.DTX[nPlayer].nノーツ数_Branch[i] * (this.fGaugeMaxRate[2] / 100.0f);
                            }
                        }
                        else
                        {
                            dbGaugeMaxComboValue = this.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[2] / 100.0f);
                        }
                        break;
                    }

                default:
                    {
                        if (this.DTX[nPlayer].bチップがある.Branch)
                        {
                            dbGaugeMaxComboValue = this.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[2] / 100.0f);
                            for (int i = 0; i < 3; i++)
                            {
                                dbGaugeMaxComboValue_branch[i] = this.DTX[nPlayer].nノーツ数[i] * (this.fGaugeMaxRate[2] / 100.0f);
                            }
                        }
                        else
                        {
                            dbGaugeMaxComboValue = this.DTX[nPlayer].nノーツ数[3] * (this.fGaugeMaxRate[2] / 100.0f);
                        }
                        break;
                    }

            }

            double nGaugeRankValue = 0D;
            double[] nGaugeRankValue_branch = new double[] { 0D, 0D, 0D };
            if (this.DTX[nPlayer].GaugeIncreaseMode == GaugeIncreaseMode.Normal)
            {
                nGaugeRankValue = Math.Floor(10000.0f / dbGaugeMaxComboValue);
                for (int i = 0; i < 3; i++)
                {
                    nGaugeRankValue_branch[i] = Math.Floor(10000.0f / dbGaugeMaxComboValue_branch[i]);
                }
            }
            else
            {
                nGaugeRankValue = 10000.0f / dbGaugeMaxComboValue;
                for (int i = 0; i < 3; i++)
                {
                    nGaugeRankValue_branch[i] = 10000.0f / dbGaugeMaxComboValue_branch[i];
                }
            }

            //ゲージ値計算
            //実機に近い計算

            this.dbゲージ増加量[0][nPlayer] = (float)nGaugeRankValue / 100.0f;
            this.dbゲージ増加量[1][nPlayer] = (float)(nGaugeRankValue / 100.0f) * 0.5f;
            this.dbゲージ増加量[2][nPlayer] = (float)(nGaugeRankValue / 100.0f) * dbDamageRate;

            for (int i = 0; i < 3; i++)
            {
                this.dbゲージ増加量_Branch[i, 0][nPlayer] = (float)nGaugeRankValue_branch[i] / 100.0f;
                this.dbゲージ増加量_Branch[i, 1][nPlayer] = (float)(nGaugeRankValue_branch[i] / 100.0f) * 0.5f;
                this.dbゲージ増加量_Branch[i, 2][nPlayer] = (float)(nGaugeRankValue_branch[i] / 100.0f) * dbDamageRate;
            }

            //2015.03.26 kairera0467 計算を初期化時にするよう修正。

            #region [ 計算結果がInfintyだった場合も考えて ]
            float fIsDontInfinty = 0.4f;//適当に0.4で
            float[] fAddVolume = new float[] { 1.0f, 0.5f, dbDamageRate };

            for (int i = 0; i < 3; i++)
            {
                for (int l = 0; l < 3; l++)
                {
                    if (!double.IsInfinity(nGaugeRankValue_branch[i] / 100.0f))//値がInfintyかチェック
                    {
                        fIsDontInfinty = (float)(nGaugeRankValue_branch[i] / 100.0f);
                        this.dbゲージ増加量_Branch[i, l][nPlayer] = fIsDontInfinty * fAddVolume[l];
                    }
                }
            }
            for (int i = 0; i < 3; i++)
            {
                for (int l = 0; l < 3; l++)
                {
                    if (double.IsInfinity(nGaugeRankValue_branch[i] / 100.0f))//値がInfintyかチェック
                    {
                        //Infintyだった場合はInfintyではない値 * 3.0をしてその値を利用する。
                        this.dbゲージ増加量_Branch[i, l][nPlayer] = (fIsDontInfinty * fAddVolume[l]) * 3f;
                    }
                }
            }
            #endregion

            #region ゲージの丸め処理
            var increase = new float[] { dbゲージ増加量[0][nPlayer], dbゲージ増加量[1][nPlayer], dbゲージ増加量[2][nPlayer] };
            var increaseBranch = new float[3, 3];
            for (int i = 0; i < 3; i++)
            {
                increaseBranch[i, 0] = dbゲージ増加量_Branch[i, 0][nPlayer];
                increaseBranch[i, 1] = dbゲージ増加量_Branch[i, 1][nPlayer];
                increaseBranch[i, 2] = dbゲージ増加量_Branch[i, 0][nPlayer];
            }
            switch (this.DTX[nPlayer].GaugeIncreaseMode)
            {
                case GaugeIncreaseMode.Normal:
                case GaugeIncreaseMode.Floor:
                    // 切り捨て
                    for (int i = 0; i < 3; i++)
                    {
                        increase[i] = (float)Math.Truncate(increase[i] * 10000.0f) / 10000.0f;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        increaseBranch[i, 0] = (float)Math.Truncate(increaseBranch[i, 0] * 10000.0f) / 10000.0f;
                        increaseBranch[i, 1] = (float)Math.Truncate(increaseBranch[i, 1] * 10000.0f) / 10000.0f;
                        increaseBranch[i, 2] = (float)Math.Truncate(increaseBranch[i, 2] * 10000.0f) / 10000.0f;
                    }
                    break;
                case GaugeIncreaseMode.Round:
                    // 四捨五入
                    for (int i = 0; i < 3; i++)
                    {
                        increase[i] = (float)Math.Round(increase[i] * 10000.0f) / 10000.0f;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        increaseBranch[i, 0] = (float)Math.Round(increaseBranch[i, 0] * 10000.0f) / 10000.0f;
                        increaseBranch[i, 1] = (float)Math.Round(increaseBranch[i, 1] * 10000.0f) / 10000.0f;
                        increaseBranch[i, 2] = (float)Math.Round(increaseBranch[i, 2] * 10000.0f) / 10000.0f;
                    }
                    break;
                case GaugeIncreaseMode.Ceiling:
                    // 切り上げ
                    for (int i = 0; i < 3; i++)
                    {
                        increase[i] = (float)Math.Ceiling(increase[i] * 10000.0f) / 10000.0f;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        increaseBranch[i, 0] = (float)Math.Ceiling(increaseBranch[i, 0] * 10000.0f) / 10000.0f;
                        increaseBranch[i, 1] = (float)Math.Ceiling(increaseBranch[i, 1] * 10000.0f) / 10000.0f;
                        increaseBranch[i, 2] = (float)Math.Ceiling(increaseBranch[i, 2] * 10000.0f) / 10000.0f;
                    }
                    break;
                case GaugeIncreaseMode.NotFix:
                default:
                    // 丸めない
                    break;
            }

            for (int i = 0; i < 3; i++)
            {
                dbゲージ増加量[i][nPlayer] = increase[i];
            }
            for (int i = 0; i < 3; i++)
            {
                dbゲージ増加量_Branch[i, 0][nPlayer] = increaseBranch[i, 0];
                dbゲージ増加量_Branch[i, 1][nPlayer] = increaseBranch[i, 1];
                dbゲージ増加量_Branch[i, 2][nPlayer] = increaseBranch[i, 2];
            }
            #endregion
        }

        #region [ DAMAGE ]
#if true       // DAMAGELEVELTUNING
        #region [ DAMAGELEVELTUNING ]
        // ----------------------------------
        public float[,] fDamageGaugeDelta = {			// #23625 2011.1.10 ickw_284: tuned damage/recover factors
			// drums,   guitar,  bass
			{  0.004f,  0.006f,  0.006f,  0.004f },
            {  0.002f,  0.003f,  0.003f,  0.002f },
            {  0.000f,  0.000f,  0.000f,  0.000f },
            { -0.020f, -0.030f, -0.030f, -0.020f },
            { -0.050f, -0.050f, -0.050f, -0.050f }
        };
        public float[] fDamageLevelFactor = {
            0.5f, 1.0f, 1.5f
        };

        public float[][] dbゲージ増加量 = new float[3][];

        //譜面レベル, 判定
        public float[,][] dbゲージ増加量_Branch = new float[3, 3][];


        public float[] fGaugeMaxRate =
        {
            70.7f,//1～7
            70f,  //8
            75.0f //9～10
        };//おおよその値。

        // ----------------------------------
        #endregion
#endif

        public void Damage(E楽器パート screenmode, E楽器パート part, E判定 e今回の判定, int nPlayer)
        {
            float fDamage;
            int nコース = (int)TJAPlayer3.stage演奏ドラム画面.n現在のコース[nPlayer];


#if true    // DAMAGELEVELTUNING
            switch (e今回の判定)
            {
                case E判定.Perfect:
                case E判定.Great:
                    {
                        if (this.DTX[nPlayer].bチップがある.Branch)
                        {
                            fDamage = this.dbゲージ増加量_Branch[nコース, 0][nPlayer];
                        }
                        else
                            fDamage = this.dbゲージ増加量[0][nPlayer];
                    }
                    break;
                case E判定.Good:
                    {
                        if (this.DTX[nPlayer].bチップがある.Branch)
                        {
                            fDamage = this.dbゲージ増加量_Branch[nコース, 1][nPlayer];
                        }
                        else
                            fDamage = this.dbゲージ増加量[1][nPlayer];
                    }
                    break;
                case E判定.Poor:
                case E判定.Miss:
                    {
                        if (this.DTX[nPlayer].bチップがある.Branch)
                        {
                            fDamage = this.dbゲージ増加量_Branch[nコース, 2][nPlayer];
                        }
                        else
                            fDamage = this.dbゲージ増加量[2][nPlayer];


                        if (fDamage >= 0)
                        {
                            fDamage = -fDamage;
                        }

                        if (this.bRisky)
                        {
                            this.nRiskyTimes--;
                        }
                    }

                    break;



                default:
                    {
                        if (nPlayer == 0 ? TJAPlayer3.ConfigIni.bAutoPlay[0] : TJAPlayer3.ConfigIni.bAutoPlay[1])
                        {
                            if (this.DTX[nPlayer].bチップがある.Branch)
                            {
                                fDamage = this.dbゲージ増加量_Branch[nコース, 0][nPlayer];
                            }
                            else
                                fDamage = this.dbゲージ増加量[0][nPlayer];
                        }
                        else
                            fDamage = 0;
                        break;
                    }


            }
#else                                                  // before applying #23625 modifications
			switch (e今回の判定)
			{
				case E判定.Perfect:
					fDamage = ( part == E楽器パート.DRUMS ) ? 0.01 : 0.015;
					break;

				case E判定.Great:
					fDamage = ( part == E楽器パート.DRUMS ) ? 0.006 : 0.009;
					break;

				case E判定.Good:
					fDamage = ( part == E楽器パート.DRUMS ) ? 0.002 : 0.003;
					break;

				case E判定.Poor:
					fDamage = ( part == E楽器パート.DRUMS ) ? 0.0 : 0.0;
					break;

				case E判定.Miss:
					fDamage = ( part == E楽器パート.DRUMS ) ? -0.035 : -0.035;
					switch( CDTXMania.ConfigIni.eダメージレベル )
					{
						case Eダメージレベル.少ない:
							fDamage *= 0.6;
							break;

						case Eダメージレベル.普通:
							fDamage *= 1.0;
							break;

						case Eダメージレベル.大きい:
							fDamage *= 1.6;
							break;
					}
					break;

				default:
					fDamage = 0.0;
					break;
			}
#endif


            this.db現在のゲージ値[nPlayer] = Math.Round(this.db現在のゲージ値[nPlayer] + fDamage, 5, MidpointRounding.ToEven);

            if (this.db現在のゲージ値[nPlayer] >= 100.0)
                this.db現在のゲージ値[nPlayer] = 100.0;
            else if (this.db現在のゲージ値[nPlayer] <= 0.0)
                this.db現在のゲージ値[nPlayer] = 0.0;

        }

        public virtual void Start(int nLane, E判定 judge, int player)
        {
        }

        //-----------------
        #endregion

        private CDTX[] DTX = new CDTX[2];
        public double[] db現在のゲージ値 = new double[4];
        protected CCounter ct炎;
        protected CCounter ct虹アニメ;
        protected CCounter ct虹透明度;
        protected CTexture[] txゲージ虹 = new CTexture[12];
        protected CTexture[] txゲージ虹2P = new CTexture[12];
    }
}
