<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    protected $procedure_name = 'SP_Calc_Miner_Depreciation_By_Id';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        $procedure = "CREATE PROCEDURE `SP_Calc_Miner_Depreciation_By_Id`(
	IN p_MemberMinerId INT
)
BEGIN
	# Depreciation 折舊
    
    /*
    礦機收益計算	礦機單日產生收益*日	
    
	礦機難度百分比	（本週期挖礦難度-上週期挖礦難度）÷上週期挖礦難度	
	礦機剩餘成本	兩週的礦機剩餘成本 = 礦機÷(1+難度百分比)	
					如遇難度百分比為負數，則礦機剩餘成本按照：
                    前一週期礦機剩餘成本-礦機兩週折舊金額計算
	礦機折舊	兩週的礦機折舊 = 礦機成本-兩週的礦機剩餘成本
	
	下一個週期礦機剩餘成本	兩週的礦機剩餘成本=礦機÷(1+難度百分比)	
	下一個週期礦機折舊	兩週的礦機折舊=礦機成本-兩週的礦機剩餘成本
	
	礦機兩週折舊金額	礦機初始成本÷預期使用年限÷12÷2	
    */
    
    # 2024/09/20 Mars 		ADD 
    
	DECLARE StartTime, EndTime, CreatedAt DATETIME;
	DECLARE MemberMinerId INT DEFAULT IFNULL(p_MemberMinerId, 0);
    
    DECLARE CurrencyId INT DEFAULT 0;
    DECLARE DepreciationCycle INT DEFAULT 0;
	DECLARE MemberId, MemberWalletId INT DEFAULT 0;
	DECLARE MinerPrice DECIMAL(30, 10) DEFAULT 0;
    # 殘值、折舊金額
    DECLARE ResidualValue, DepreciationAmount, DefaultDepreciationAmount DECIMAL(30, 10) DEFAULT 0;
    # 下一個剩餘值、下一個折舊金額
    DECLARE NextResidualValue, NextDepreciationAmount DECIMAL(30, 10) DEFAULT 0;
    DECLARE MinerType, RewardType INT DEFAULT 1; 
	DECLARE DepreciationInfo TEXT;
    
	DECLARE MiningBlockId INT DEFAULT 0;
	# 挖礦難度
	DECLARE MiningDifficulty, LastMiningDifficulty DECIMAL(10, 5) DEFAULT 0;
	# 難度增幅
	DECLARE MiningChange, LastMiningChange DECIMAL(5, 2) DEFAULT 0;
	# 難度百分比
	DECLARE DifficultyPercentage DECIMAL(10, 5) DEFAULT 0;
	# 剩餘成本
	#DECLARE RemainingCost;
    
	DECLARE Done INT DEFAULT 0;
    
    # 取得需算折舊的礦機
	DECLARE CurDate CURSOR FOR 
		SELECT z.member_miner_id, 
			z.type, 
            z.member_id, 
			z.miner_price, 
            (z.miner_price / 3 / 12 / 2) AS depreciation_amount,
            z.currency_id, 
            z.start_time, 
            z.end_time
		FROM (
			SELECT mm.id AS member_miner_id, mm.type, mm.member_id
				, CASE WHEN mm.type = 2 THEN group_price ELSE purchase_price END AS miner_price
                , m.currency_id 
                , m.depreciation_cycle
                , mm.start_time
                , mm.end_time
			FROM member_miners AS mm
			JOIN miners AS m ON m.id = mm.miner_id
            #WHERE mm.id = MemberMinerId
			WHERE (mm.id = MemberMinerId OR MemberMinerId = 0)
            AND mm.type IN (1, 2)
        ) AS z;
        
    ### SQL異常時發TG通知 ##################
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 
			@p2 = MESSAGE_TEXT;
            
		ROLLBACK;
        # 發通知
        CALL COM_Send_Error_Message(
			CONCAT(
				'[系統通知] SP_Calc_Miner_Depreciation_By_Id(',
                p_MemberMinerId,
				') 異常 ', @p2), 
			'db'
		);
    END;
    
	DECLARE CONTINUE HANDLER FOR NOT FOUND SET Done = 1;
    
    START TRANSACTION;
    
    OPEN CurDate;
	REPEAT
		FETCH CurDate 
        INTO MemberMinerId, MinerType, MemberId, 
			MinerPrice, DefaultDepreciationAmount, CurrencyId, StartTime, EndTIme;
		IF NOT Done THEN
			SET CreatedAt = StartTime;
            
			DELETE FROM member_miner_depreciations WHERE member_miner_id = MemberMinerId;
            
            BlockSub: BEGIN
			DECLARE BlockTime DATETIME;
			DECLARE DoneSub INT DEFAULT 0;
                
			DECLARE curSub CURSOR FOR 
				SELECT id, mining_difficulty, mining_change, block_time 
				FROM mining_blocks 
				WHERE currency_id = CurrencyId 
				AND block_time >= DATE_SUB(StartTime, INTERVAL 14 DAY)
				AND status = 1
				ORDER BY block_height ASC;
                    
			DECLARE CONTINUE HANDLER FOR NOT FOUND SET DoneSub = 1;
            OPEN curSub;
			REPEAT
			FETCH curSub 
			INTO MiningBlockId, MiningDifficulty, MiningChange, BlockTime;
			IF NOT DoneSub THEN
				# 下期折舊
				SET NextDepreciationAmount = DefaultDepreciationAmount;
                SET DifficultyPercentage = 0;
                
				# 如果有折舊紀錄, 就用最後一筆的殘值來當礦機餘額
				IF EXISTS (
					SELECT * FROM member_miner_depreciations 
					WHERE member_miner_id = MemberMinerId
				) THEN
					SELECT ResidualValue 
					INTO MinerPrice
					FROM member_miner_depreciations 
					WHERE member_miner_id = MemberMinerId
					ORDER BY id DESC
					LIMIT 1;
				END IF;
            
				# 上次殘值大於0才做計算
				IF MinerPrice > 0 THEN
                
					SELECT mining_difficulty, mining_change 
					INTO LastMiningDifficulty, LastMiningChange
					FROM mining_blocks 
					WHERE currency_id = CurrencyId AND status = 1
					AND id < MiningBlockId
					ORDER BY block_height DESC LIMIT 1;
                    
					# 礦機難度百分比 =（本週期挖礦難度-上週期挖礦難度）÷上週期挖礦難度
					SET DifficultyPercentage = (MiningDifficulty - LastMiningDifficulty) / LastMiningDifficulty;
					
					# 計算 礦機剩餘成本
					IF DifficultyPercentage < 0 THEN 
						# 如遇難度百分比為負數，則礦機剩餘成本按照：
						# 前一週期礦機剩餘成本-礦機兩週折舊金額計算
						SET ResidualValue = MinerPrice - DefaultDepreciationAmount;
                        #SELECT '1', DifficultyPercentage, (MiningDifficulty - LastMiningDifficulty), LastMiningDifficulty, MinerPrice, ResidualValue;
					ELSE	
						SET ResidualValue = MinerPrice / (1 + DifficultyPercentage);
                        #SELECT '2', DifficultyPercentage, (MiningDifficulty - LastMiningDifficulty), LastMiningDifficulty, MinerPrice, ResidualValue;
					END IF;
					
					# 礦機折舊	兩週的礦機折舊 = 礦機成本-兩週的礦機剩餘成本
					SET DepreciationAmount = MinerPrice - ResidualValue;
				
					# 計算下期殘值
					SET NextResidualValue = ResidualValue - DepreciationAmount;
					
					# 當期殘值小於0時
					IF ResidualValue < 0 THEN
						SET ResidualValue = 0;
						SET DepreciationAmount = MinerPrice;
						SET NextResidualValue = 0;
						SET NextDepreciationAmount = 0;
					ELSEIF NextResidualValue < 0 THEN
						# 下期殘值小於0時
						SET NextResidualValue = 0;
						SET NextDepreciationAmount = ResidualValue;
					END IF;
					
					SET DepreciationInfo = CAST(JSON_OBJECT(
						'MiningBlockId', MiningBlockId, 
						'MiningDifficulty', MiningDifficulty, 
                        'LastMiningDifficulty', LastMiningDifficulty,
						'DifficultyPercentage', DifficultyPercentage
					) AS CHAR);
                    
					# 寫入紀錄
					INSERT INTO member_miner_depreciations(
						member_miner_id, mining_block_id, 
						residual_value, depreciation_amount, # 殘值, 折舊金額
						next_residual_value, next_depreciation_amount, # 下期殘值, 下期折舊金額
						info, created_at, updated_at
					)
					VALUES(
						MemberMinerId, MiningBlockId,
						ResidualValue, DepreciationAmount,
						NextResidualValue, NextDepreciationAmount,
						IFNULL(DepreciationInfo, ''), CreatedAt, CreatedAt
					);
				END IF;
                
				SET CreatedAt = DATE_ADD(StartTime, INTERVAL 14 DAY);
            
			END IF;
            UNTIL DoneSub END REPEAT;
			CLOSE curSub;
            
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
