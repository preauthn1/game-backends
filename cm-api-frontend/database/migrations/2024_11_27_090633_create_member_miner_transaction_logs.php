<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'member_miner_transaction_logs';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->integer('transaction_log_id')->comment('交易紀錄系統編號(transaction_logs.id)');
                $table->integer('member_miner_id')->comment('會員礦機系統編號(member_miners.id)');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //unique
                $table->unique(['transaction_log_id', 'member_miner_id'], 'uni_member_miner_transaction_logs_1');
                //index
                $table->index('transaction_log_id', 'idx_' . $this->table_name . '_' . 'transaction_log_id');
                $table->index('member_miner_id', 'idx_' . $this->table_name . '_' . 'member_miner_id');
            });
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
