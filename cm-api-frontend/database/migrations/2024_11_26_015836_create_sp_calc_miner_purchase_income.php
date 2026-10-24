<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    protected $procedure_name = 'SP_Calc_Miner_Purchase_Income';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        $procedure = "CREATE PROCEDURE `SP_Calc_Miner_Purchase_Income`(
)
BEGIN
	# Purchase income 購買/團購收益
    # 2024/09/206 Mars 		ADD 
    
    DECLARE Today DATE 
		DEFAULT DATE(DATE_ADD(NOW(), INTERVAL 8 HOUR));
    DECLARE Yesterday DATE 
		DEFAULT DATE_SUB(Today, INTERVAL 1 DAY);
	DECLARE StartTime DATETIME 
		DEFAULT DATE_SUB(Yesterday, INTERVAL 8 HOUR);
    DECLARE EndTime DATETIME 
		DEFAULT DATE_SUB(DATE_SUB(Today, INTERVAL 8 HOUR), INTERVAL 1 SECOND);
	
	DECLARE MemberMinerId, MemberMinerRewardId, TransactionLogId INT DEFAULT 0;
	DECLARE MemberId, MemberWalletId INT DEFAULT 0;
	DECLARE DayRevenue DECIMAL(30, 10) DEFAULT 0;
    DECLARE MinerType, RewardType INT DEFAULT 1; 

	DECLARE Done INT DEFAULT 0;
    
    # 運行中礦機
	DECLARE CurDate CURSOR FOR 
		SELECT mm.id, mm.type, mm.member_id
			, CASE WHEN mm.type = 2 THEN TRUNCATE(m.day_revenue / 2, 2) ELSE m.day_revenue END day_revenue
		FROM member_miners AS mm
		JOIN miners AS m ON m.id = mm.miner_id
		WHERE mm.start_time <= StartTime
		AND mm.end_time >= EndTime
        AND mm.type IN (1, 2);
        
    ### SQL異常時發TG通知 ##################
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 
			@p2 = MESSAGE_TEXT;
            
		ROLLBACK;
        # 發通知
        CALL COM_Send_Error_Message(
			CONCAT(
				'[系統通知] SP_Calc_Miner_Purchase_Income(',
				') 異常 ', @p2), 
			'db'
		);
    END;
    
	DECLARE CONTINUE HANDLER FOR NOT FOUND SET Done = 1;
    
    START TRANSACTION;
    
    OPEN CurDate;
	REPEAT
		FETCH CurDate 
        INTO MemberMinerId, MinerType, MemberId, DayRevenue;
		IF NOT Done THEN
			
            INSERT INTO member_miner_incomes(
				type, summary_time, data_time, member_id, member_miner_id, amount
			)
            VALUES(
				MinerType, StartTime, StartTime, MemberId, MemberMinerId, DayRevenue
            );
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
