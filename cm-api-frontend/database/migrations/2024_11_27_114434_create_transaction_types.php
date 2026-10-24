<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'transaction_types';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->string('name', 191)->collation('utf8mb4_unicode_ci')->comment('單號');
                $table->tinyInteger('type')->default(0)->comment('交易類型');
                $table->string('memo', 500)->collation('utf8mb4_unicode_ci')->nullable();
                $table->text('remark')->collation('utf8mb4_unicode_ci')->comment('備註');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('type', 'idx_transaction_types_type');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'name' => 'Deposit', 'type' => 1, 'memo' => '入分', 'remark' => 'deposit_orders.id'],
                ['id' => 2, 'name' => 'Withdrawal', 'type' => 1, 'memo' => '出分', 'remark' => 'withdrawal_orders.id'],
                ['id' => 3, 'name' => 'Purchase Mining', 'type' => 1, 'memo' => '購買/團購礦機', 'remark' => 'member_miners.id'],
                ['id' => 4, 'name' => 'Rental Mining', 'type' => 1, 'memo' => '租賃礦機', 'remark' => 'member_miners.id'],
                ['id' => 5, 'name' => 'Cancel Group Mining', 'type' => 1, 'memo' => '取消團購礦機', 'remark' => 'member_miners.id'],
                ['id' => 6, 'name' => 'Sell Mining', 'type' => 1, 'memo' => '賣回礦機(購買/團購)', 'remark' => 'member_miners.id'],
                ['id' => 7, 'name' => 'Mining Profits', 'type' => 1, 'memo' => '挖礦利潤', 'remark' => 'member_miners.id'],
                ['id' => 8, 'name' => 'Group Mining Profits', 'type' => 1, 'memo' => '挖礦利潤(團購)', 'remark' => 'member_miners.id'],
                ['id' => 9, 'name' => 'Rental Mining Profits', 'type' => 1, 'memo' => '挖礦利潤(租賃)', 'remark' => 'member_miners.id'],
                ['id' => 10, 'name' => 'Referral Rewards', 'type' => 1, 'memo' => '推薦獎勵', 'remark' => 'member_miner_rewards.id'],
                ['id' => 11, 'name' => 'Sales Rebate', 'type' => 1, 'memo' => '分銷獎勵', 'remark' => 'member_miner_rebates.id'],
                ['id' => 12, 'name' => 'Manual Add', 'type' => 1, 'memo' => '人工上分', 'remark' => ''],
                ['id' => 13, 'name' => 'Manual Sub', 'type' => 1, 'memo' => '人工下分', 'remark' => ''],
                ['id' => 14, 'name' => 'Return Withdrawal', 'type' => 1, 'memo' => '洗分退還', 'remark' => 'withdrawal_orders.id'],
            ]);
        }
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists($this->table_name);
    }
};
