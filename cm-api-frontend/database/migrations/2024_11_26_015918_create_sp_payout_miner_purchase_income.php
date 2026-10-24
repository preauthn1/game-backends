<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    protected $procedure_name = 'SP_Payout_Miner_Purchase_Income';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        $procedure = "CREATE PROCEDURE `SP_Payout_Miner_Purchase_Income`(
)
BEGIN
	# 派發 購買/團購收益
    # 2024/09/206 Mars 		ADD 
    
	DECLARE MemberMinerIncomeId, TransactionLogId INT DEFAULT 0;
	DECLARE MemberId, MemberWalletId INT DEFAULT 0;
	DECLARE RewardAmount, BeforeBalance, AfterBalance DECIMAL(30, 10) DEFAULT 0;
    DECLARE Operator INT DEFAULT 51;
    DECLARE WalletTypeId INT DEFAULT 1;
    DECLARE RewardType INT DEFAULT 1; 
    # 1:充值, 2:提領, 3:購買/團購礦機, 4:租賃礦機, 5:取消購買/團購礦機, 6:賣回礦機, 
    # 7:挖礦利潤, 8:挖礦利潤(團購), 9:挖礦利潤(租賃), 10:推薦獎勵, 11:分銷獎勵
    # 12:人工上分, 13:人工下分
    DECLARE TransactionTypeId INT DEFAULT 7;
	DECLARE Done INT DEFAULT 0;
    
    # 獲取未派發獎勵
	DECLARE CurDate CURSOR FOR 
		SELECT id, type, member_id, amount
		FROM member_miner_incomes
		WHERE status = 0
        AND type IN (1, 2);
        
    ### SQL異常時發TG通知 ##################
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        GET DIAGNOSTICS CONDITION 1 
			@p2 = MESSAGE_TEXT;
            
		ROLLBACK;
        # 發通知
        CALL COM_Send_Error_Message(
			CONCAT(
				'[系統通知] SP_Payout_Miner_Purchase_Income(',
				') 異常 ', @p2), 
			'db'
		);
    END;
    
	DECLARE CONTINUE HANDLER FOR NOT FOUND SET Done = 1;
    
    START TRANSACTION;
    OPEN CurDate;
	REPEAT
		FETCH CurDate 
        INTO MemberMinerIncomeId, RewardType, MemberId, RewardAmount;
		IF NOT Done THEN
			
            IF RewardType = 1 THEN
				SET TransactionTypeId = 7;
            ELSE
				SET TransactionTypeId = 8;
            END IF;

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
				WalletTypeId, MemberId, TransactionTypeId, MemberMinerIncomeId, 
				RewardAmount, BeforeBalance, (BeforeBalance + RewardAmount), Operator, 0
			);
			SELECT LAST_INSERT_ID() INTO TransactionLogId;
            
			# 寫入member_wallet
			UPDATE member_wallets
			SET balance = (balance + RewardAmount)
			WHERE id = MemberWalletId;
            
			# 更新transaction_logs.status
			UPDATE transaction_logs
			SET status = 1
			WHERE id = TransactionLogId;
            
			# 更新member_miner_incomes.status
			UPDATE member_miner_incomes
			SET status = 1
			WHERE id = MemberMinerIncomeId;
    
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
