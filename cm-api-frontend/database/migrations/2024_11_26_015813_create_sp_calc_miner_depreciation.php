<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    protected $procedure_name = 'SP_Calc_Miner_Depreciation';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        $procedure = "CREATE PROCEDURE `SP_Calc_Miner_Depreciation`(
	IN p_CalcDate DATE
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
    
	DECLARE Today DATE 
		DEFAULT IFNULL(p_CalcDate, DATE(DATE_ADD(NOW(), INTERVAL 8 HOUR)));
    DECLARE Yesterday DATE 
		DEFAULT DATE_SUB(Today, INTERVAL 1 DAY);
	DECLARE StartTime DATETIME 
		DEFAULT DATE_SUB(Yesterday, INTERVAL 8 HOUR);
    DECLARE EndTime DATETIME 
		DEFAULT DATE_SUB(DATE_SUB(Today, INTERVAL 8 HOUR), INTERVAL 1 SECOND);
	
	DECLARE MemberMinerId, MiningBlockId INT DEFAULT 0;
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
            z.depreciation_cycle
		FROM (
			SELECT mm.id AS member_miner_id, mm.type, mm.member_id
				, CASE WHEN mm.type = 2 THEN group_price ELSE purchase_price END AS miner_price
                , m.currency_id 
                , m.depreciation_cycle
				, MOD(DATEDIFF(EndTime, mm.start_time), m.depreciation_cycle) AS remainder
			FROM member_miners AS mm
			JOIN miners AS m ON m.id = mm.miner_id
			WHERE mm.start_time <= StartTime
			AND mm.end_time >= EndTime
			AND mm.type IN (1, 2)
            #AND mm.id in (1418, 1419, 1420, 1421, 1422, 1423, 1424, 1425, 1426, 1427)
		) AS z
		WHERE z.remainder = 0
		ORDER BY z.type ASC, z.member_miner_id;
        
    ### SQL異常時發TG通知 ##################
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 
			@p2 = MESSAGE_TEXT;
            
		ROLLBACK;
        # 發通知
        CALL COM_Send_Error_Message(
			CONCAT(
				'[系統通知] SP_Calc_Miner_Depreciation(',
                p_CalcDate,
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
			MinerPrice, DefaultDepreciationAmount, CurrencyId, DepreciationCycle;
		IF NOT Done THEN
        
            # 下期折舊
            SET NextDepreciationAmount = DefaultDepreciationAmount;
            
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
				# 計算當期折舊
				SELECT id, mining_difficulty, mining_change 
				INTO MiningBlockId, MiningDifficulty, MiningChange
				FROM mining_blocks 
				WHERE currency_id = CurrencyId AND status = 1
				ORDER BY block_height DESC LIMIT 1;
                
                IF NOT EXISTS (
					SELECT * FROM member_miner_depreciations 
					WHERE member_miner_id = MemberMinerId
                    AND mining_block_id = MiningBlockId
				) THEN
					SELECT mining_difficulty, mining_change 
					INTO LastMiningDifficulty, LastMiningChange
					FROM mining_blocks 
					WHERE currency_id = CurrencyId AND status = 1
					AND id != MiningBlockId
					ORDER BY block_height DESC LIMIT 1;
		
					# 礦機難度百分比 =（本週期挖礦難度-上週期挖礦難度）÷上週期挖礦難度
					SET DifficultyPercentage = (MiningDifficulty - LastMiningDifficulty) / LastMiningDifficulty;
					
					# 計算 礦機剩餘成本
					IF DifficultyPercentage < 0 THEN 
						# 如遇難度百分比為負數，則礦機剩餘成本按照：
						# 前一週期礦機剩餘成本-礦機兩週折舊金額計算
						SET ResidualValue = MinerPrice - DefaultDepreciationAmount;
					ELSE
						SET ResidualValue = MinerPrice / (1 + DifficultyPercentage);
					END IF;
					
					# 礦機折舊	兩週的礦機折舊 = 礦機成本-兩週的礦機剩餘成本
					SET DepreciationAmount = MinerPrice - ResidualValue;
			
					# 計算下期殘值
					SET NextResidualValue = ResidualValue - DefaultDepreciationAmount;
					
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
						'DifficultyPercentage', DifficultyPercentage
					) AS CHAR);
				
					# 寫入紀錄
					INSERT INTO member_miner_depreciations(
						member_miner_id, mining_block_id, 
						residual_value, depreciation_amount, # 殘值, 折舊金額
						next_residual_value, next_depreciation_amount, # 下期殘值, 下期折舊金額
						info
					)
					VALUES(
						MemberMinerId, MiningBlockId,
						ResidualValue, DepreciationAmount,
						NextResidualValue, NextDepreciationAmount,
						IFNULL(DepreciationInfo, '')
					);
                END IF;
            END IF;
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
