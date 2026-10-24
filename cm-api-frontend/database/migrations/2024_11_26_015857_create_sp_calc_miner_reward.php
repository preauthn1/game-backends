<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    protected $procedure_name = 'SP_Calc_Miner_Reward';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        $procedure = "CREATE PROCEDURE `SP_Calc_Miner_Reward`(
	IN p_CalcDate DATE
)
BEGIN
	# Reward 邀請活動
    # 每一種型號的直屬邀請人數獎勵設置，
    # 同一型號礦機第總滿3人、總滿5人、總滿10人的分別獎勵
    # （拼單不計算有效邀請人數，需要整機訂單才計算有效人數1）；
    # 新增上架礦機時也需要可以設置邀請獎勵設置
    
    # 2024/09/21 Mars 		ADD 
    
    DECLARE RewardType INT DEFAULT 1; 
    DECLARE MinerRewardId, MinerId, MinerRewardQuantity INT DEFAULT 0;
    DECLARE RewardAmount DECIMAL(30, 10) DEFAULT 0;
    DECLARE SummaryTime DATETIME
		DEFAULT DATE_ADD(DATE(DATE_ADD(NOW(), INTERVAL 8 HOUR)), INTERVAL 4 HOUR);
    DECLARE CalcDate DATE
		DEFAULT IFNULL(p_CalcDate, DATE_SUB(DATE(DATE_ADD(NOW(), INTERVAL 8 HOUR)), INTERVAL 1 DAY));
    DECLARE DataTime DATETIME
		DEFAULT DATE_SUB(CalcDate, INTERVAL 8 HOUR);
    
	DECLARE Done INT DEFAULT 0;
	
    # 獲取獎勵
	DECLARE CurDate CURSOR FOR 
		SELECT id, miner_id, quantity, amount 
        FROM miner_rewards
        WHERE status = 1
        ORDER BY miner_id ASC, quantity ASC;
    
    ### SQL異常時發TG通知 ##################
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 
			@p2 = MESSAGE_TEXT;
            
		ROLLBACK;
        # 發通知
        CALL COM_Send_Error_Message(
			CONCAT(
				'[系統通知] SP_Calc_Miner_Reward(',
                IFNULL(p_CalcDate, 'NULL'), 
				') 異常 ', @p2), 
			'db'
		);
    END;
    
	DECLARE CONTINUE HANDLER FOR NOT FOUND SET Done = 1;
    
    START TRANSACTION;
    
    OPEN CurDate;
	REPEAT
		FETCH CurDate 
        INTO MinerRewardId, MinerId, MinerRewardQuantity, RewardAmount;
		IF NOT Done THEN
			
            BlockSub: BEGIN
            DECLARE MemberId, MinerCount INT DEFAULT 0;
			DECLARE DoneSub INT DEFAULT 0;
    
			DECLARE CurSub CURSOR FOR 
				SELECT z.member_id, z.cnt
				FROM (
					SELECT p.id as member_id, mm.miner_id, count(*) AS cnt
					FROM (
						SELECT member_id, miner_id, COUNT(*) AS cnt
						FROM member_miners
						WHERE type = 1
                        OR (type = 2 AND start_time IS NOT NULL)
						AND miner_id = MinerId
						GROUP BY member_id, miner_id
					) AS mm
					JOIN members AS m 
						ON m.id = mm.member_id 
                        AND m.type = 1
					JOIN members AS p 
						ON p.invitation_code = m.referral_code
                        AND p.type = 1
					GROUP BY p.id, mm.miner_id
					ORDER BY mm.miner_id, p.id, cnt
				) AS z
				LEFT JOIN member_miner_rewards AS mmr 
					ON mmr.member_id = z.member_id
					AND mmr.miner_reward_id = MinerRewardId
				WHERE z.cnt >= MinerRewardQuantity
				AND mmr.id IS NULL;
                
			DECLARE CONTINUE HANDLER FOR NOT FOUND SET DoneSub = 1;
			
			OPEN CurSub;
			REPEAT
			FETCH CurSub 
			INTO MemberId, MinerCount;
				IF NOT DoneSub THEN
					# 寫入獎勵; member_miner_rewards
					INSERT INTO member_miner_rewards(
						type, summary_time, data_time, member_id, miner_reward_id, amount
					)
					VALUES (
						RewardType, SummaryTime, SummaryTime, MemberId, MinerRewardId, RewardAmount
					);
                    
				END IF;
			UNTIL DoneSub END REPEAT;
			CLOSE CurSub;
			END BlockSub;
        END IF;
    UNTIL Done END REPEAT;
    CLOSE CurDate;
	COMMIT;
END";
        DB::unprepared('DROP procedure IF EXISTS ' . $this->procedure_name);
        DB::unprepared($procedure);
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        DB::unprepared('DROP procedure IF EXISTS ' . $this->procedure_name);
    }
};
