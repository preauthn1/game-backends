<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    protected $procedure_name = 'SP_Payout_Miner_Rebate';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        $procedure = "CREATE PROCEDURE `SP_Payout_Miner_Rebate`(
)
BEGIN
	# 派發代理分銷
    # 2024/09/206 Mars 		ADD 
    
	DECLARE MemberMinerRebateId, TransactionLogId INT DEFAULT 0;
	DECLARE MemberId, MemberWalletId INT DEFAULT 0;
	DECLARE RebateAmount, BeforeBalance, AfterBalance DECIMAL(30, 10) DEFAULT 0;
    DECLARE Operator INT DEFAULT 51;
    DECLARE WalletTypeId INT DEFAULT 1;
    DECLARE RewardType INT DEFAULT 1; 
    # 1:進分, 2:出分, 3:購買/團購礦機, 4:租賃礦機, 5:取消購買/團購礦機, 6:賣回礦機, 
    # 7:挖礦利潤, 8:挖礦利潤(團購), 9:挖礦利潤(租賃), 10:推薦獎勵, 11:分銷獎勵
    DECLARE TransactionTypeId INT DEFAULT 11;
	DECLARE Done INT DEFAULT 0;
    
    # 獲取未派發獎勵
	DECLARE CurDate CURSOR FOR 
		SELECT id, member_id, amount
		FROM member_miner_rebates
		WHERE status = 0;
        
    ### SQL異常時發TG通知 ##################
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 
			@p2 = MESSAGE_TEXT;
            
		ROLLBACK;
        # 發通知
        CALL COM_Send_Error_Message(
			CONCAT(
				'[系統通知] SP_Payout_Miner_Rebate(',
				') 異常 ', @p2), 
			'db'
		);
    END;
    
	DECLARE CONTINUE HANDLER FOR NOT FOUND SET Done = 1;
    
    START TRANSACTION;
    OPEN CurDate;
	REPEAT
		FETCH CurDate 
        INTO MemberMinerRebateId, MemberId, RebateAmount;
		IF NOT Done THEN

			# 取得玩家當前額度
			SELECT id, balance 
			INTO MemberWalletId, BeforeBalance 
			FROM member_wallets 
			WHERE member_id = MemberId
			AND wallet_type_id = WalletTypeId;
            
			# 寫入transaction_log
			INSERT INTO transaction_logs(
				Wallet_type_id, member_id, transaction_type_id, order_id, 
				point, before_balance, after_balance, operator, status
			)
			VALUES (
				WalletTypeId, MemberId, TransactionTypeId, MemberMinerRebateId, 
				RebateAmount, BeforeBalance, (BeforeBalance + RebateAmount), Operator, 0
			);
			SELECT LAST_INSERT_ID() INTO TransactionLogId;
            
			# 寫入member_wallet
			UPDATE member_wallets
			SET balance = (balance + RebateAmount)
			WHERE id = MemberWalletId;
            
			# 更新transaction_logs.status
			UPDATE transaction_logs
			SET status = 1
			WHERE id = TransactionLogId;
            
			# 更新member_miner_rebates.status
			UPDATE member_miner_rebates
			SET status = 1
			WHERE id = MemberMinerRebateId;
    
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
