<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    protected $procedure_name = 'SP_Calc_Miner_Rebate';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        $procedure = "CREATE PROCEDURE `SP_Calc_Miner_Rebate`(
	IN p_CheckDate DATE
)
BEGIN
	# Rebate 代理分銷
    # 每一種礦機型號的一～三級分別的銷售返佣設置，
    # 單位為百分比；
    # 每一種礦機型號邀請第1～第3人、第4～第5人、第6之後開始的
    # 所有三級用戶銷售返佣百分比；
    # 新增上架礦機時也需要可以設置三級分銷的返佣百分比
    
    # 2024/09/206 Mars 		ADD 
    
    DECLARE MaxLevel INT DEFAULT (
		SELECT MAX(z.level) FROM (SELECT level FROM members GROUP BY level) AS z
    );
    DECLARE ThisLevel INT DEFAULT 0;
    DECLARE MinerId INT DEFAULT 0;
    DECLARE RebatePercentage1, RebatePercentage2, RebatePercentage3 
		DECIMAL(5, 4) DEFAULT 0;
    DECLARE PurchasePrice, GroupPrice DECIMAL(30, 10) DEFAULT 0;
        
	DECLARE Today DATE DEFAULT DATE(DATE_ADD(NOW(), INTERVAL 8 HOUR)); 
    DECLARE SummaryTime DATETIME
		DEFAULT DATE_SUB(Today, INTERVAL 8 HOUR);
    DECLARE CheckDate DATE DEFAULT IFNULL(p_CheckDate, Today);
    DECLARE DataTime DATETIME DEFAULT DATE_SUB(CheckDate, INTERVAL 8 HOUR);
	DECLARE Done INT DEFAULT 0;
	
    # 獲取獎勵
	DECLARE CurDate CURSOR FOR 
		SELECT miner.id AS miner_id
			, miner.purchase_price
            , miner.group_price
			, mr.percentage1 
			, mr.percentage2 
			, mr.percentage3 
        FROM miner_rebates AS mr
        JOIN miners AS miner 
			ON miner.id = mr.miner_id 
            AND miner.status = 1
        WHERE mr.status = 1
        ORDER BY miner.id ASC;
    
    ### SQL異常時發TG通知 ##################
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 
			@p2 = MESSAGE_TEXT;
            
		ROLLBACK;
        # 發通知
        CALL COM_Send_Error_Message(
			CONCAT(
				'[系統通知] SP_Calc_Miner_Rebate(',
				') 異常 ', @p2), 
			'db'
		);
    END;
    
	DECLARE CONTINUE HANDLER FOR NOT FOUND SET Done = 1;
    
    START TRANSACTION;
    
    /*
    每天12:00計算 13:00~13:05執行派發 統計前一天的
	1. 礦機收益 每天發
	3. 三級返佣 按等級配置比例 自己0%/直屬2%/二級1%
    
    */
    # 先算出個人累積台數
    OPEN CurDate;
	REPEAT
		FETCH CurDate 
        INTO MinerId, PurchasePrice, GroupPrice, RebatePercentage1, RebatePercentage2, RebatePercentage3;
		IF NOT Done THEN
			# 初始ThisLevel
			SET ThisLevel = MaxLevel;
        
			WHILE ThisLevel > 0
			DO
				INSERT INTO member_miner_temps(
					summary_time, data_time
					, member_id, parent_id, miner_id, `type`
                    , self_count, self_amount
                    , direct_count, direct_amount, direct_info
                    , team_count, team_amount, team_info
				)
                SELECT SummaryTime, DataTime
					, z.member_id
					, z.parent_id
					, z.miner_id
					, z.type
                    , z.self_count
                    , z.self_amount
                    , z.direct_count
                    , z.direct_amount
                    , z.direct_info
                    , z.team_count
                    , z.team_amount
                    , z.team_info
                FROM (
					SELECT m.id AS member_id
						, IFNULL(p.id, 0) AS parent_id
                        , z.miner_id
                        , z.type
                        , z.cnt AS self_count
                        , z.amount AS self_amount
                        , IFNULL(SUM(mmt.self_count), 0) AS direct_count
                        , IFNULL(SUM(mmt.self_amount), 0) AS direct_amount
						, GROUP_CONCAT(DISTINCT mmt.member_id) AS direct_info
                        , IFNULL(SUM(mmt.direct_count), 0) AS team_count
                        , IFNULL(SUM(mmt.direct_amount), 0) AS team_amount
						, GROUP_CONCAT(DISTINCT mmt.direct_info) AS team_info
					FROM (
						SELECT mm.miner_id
							, mm.member_id
                            , mm.type
							, SUM(CASE mm.type WHEN 1 THEN 1 WHEN 2 THEN 0.5 ELSE 0 END) AS cnt
                            , SUM(CASE mm.type WHEN 1 THEN miner.purchase_price WHEN 2 THEN miner.group_price ELSE 0 END) AS amount
						FROM member_miners AS mm
						JOIN miners AS miner 
							ON miner.id = mm.miner_id
						WHERE mm.miner_id = MinerId
                        AND mm.start_time = DataTime
						AND mm.type IN (1, 2)
						AND mm.start_time IS NOT NULL
						GROUP BY mm.member_id, mm.type
					) AS z
					JOIN members AS m 
						ON m.id = z.member_id 
						AND m.level = ThisLevel
					LEFT JOIN members AS p 
						ON p.invitation_code = m.referral_code 
					LEFT JOIN member_miner_temps AS mmt 
						ON mmt.parent_id = z.member_id
                        AND mmt.miner_id = MinerId
					WHERE z.cnt > 0
					GROUP BY z.member_id, z.miner_id
                ) AS z
				ON DUPLICATE KEY UPDATE 
					self_count = z.self_count
					, self_amount = z.self_amount
					, direct_count = z.direct_count
					, direct_amount = z.direct_amount
                    , direct_info = z.direct_info
					, team_count = z.team_count
					, team_amount = z.team_amount
                    , team_info = z.team_info;
            
				SET ThisLevel = ThisLevel - 1;
			END WHILE;
        
        END IF;
    UNTIL Done END REPEAT;
    CLOSE CurDate;
	COMMIT;
    
    # 下級
    INSERT INTO member_miner_rebates(summary_time, data_time, `type`, member_id, miner_id, amount, info)
	SELECT SummaryTime, DataTime
		, mmt.type
		, mmt.member_id, mmt.miner_id
		, IFNULL(TRUNCATE(mmt.direct_amount * mr.percentage2, 2), 0) AS direct_rebate
		, JSON_OBJECT(
			'price', 0 + CAST(mmt.direct_amount AS CHAR),
			'percentage', 0 + CAST(mr.percentage2 AS CHAR),
			'amount', IFNULL(TRUNCATE(mmt.direct_amount * mr.percentage2, 2), 0),
			'member_ids', direct_info,
			'level', 'direct'
		) AS json_data
	FROM member_miner_temps as mmt
	JOIN miner_rebates as mr 
		ON mr.miner_id = mmt.miner_id
		AND mr.status = 1
	LEFT JOIN member_miner_rebates AS mmr
		ON mmr.member_id = mmt.member_id
		AND mmr.summary_time = mmt.summary_time
		AND mmr.miner_id = mmt.miner_id
		AND mmr.data_time = mmt.data_time
		AND mmr.type = mmt.type
	WHERE mmt.summary_time = SummaryTime
    AND mmt.direct_amount > 0
	AND mmr.id IS NULL
	ORDER BY mmt.miner_id ASC, mmt.member_id ASC;
  
    # 下下級獎勵
    INSERT INTO member_miner_rebates(summary_time, data_time, `type`, member_id, miner_id, amount, info)
	SELECT SummaryTime, DataTime
		, mmt.type
		, mmt.member_id, mmt.miner_id
		, IFNULL(TRUNCATE(mmt.team_amount * mr.percentage3, 2), 0) AS team_rebate
		, JSON_OBJECT(
			'price', 0 + CAST(mmt.team_amount AS CHAR),
			'percentage', 0 + CAST(mr.percentage3 AS CHAR),
			'amount', IFNULL(TRUNCATE(mmt.team_amount * mr.percentage3, 2), 0),
			'member_ids', team_info,
			'level', 'team'
		) AS json_data
	FROM member_miner_temps as mmt
	JOIN miner_rebates as mr 
		ON mr.miner_id = mmt.miner_id
		AND mr.status = 1
	LEFT JOIN member_miner_rebates AS mmr
		ON mmr.member_id = mmt.member_id
		AND mmr.summary_time = SummaryTime
		AND mmr.miner_id = mmt.miner_id
		AND mmr.data_time = mmt.data_time
		AND mmr.type = mmt.type
	WHERE mmt.summary_time = SummaryTime
    AND mmt.team_amount > 0
	AND mmr.id IS NULL
	ORDER BY mmt.miner_id ASC, mmt.member_id ASC;
    
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
