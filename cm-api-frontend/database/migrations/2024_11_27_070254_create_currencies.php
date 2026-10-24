<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'currencies';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->integer('organization_id')->default(1)->comment('所屬組織');
                $table->tinyInteger('type')->default(0)->comment('1:平台使用, 2:法幣, 3:加密貨幣');
                $table->string('code', 10)->collation('utf8mb4_unicode_ci')->comment('貨幣代碼');
                $table->string('name', 32)->collation('utf8mb4_unicode_ci')->comment('貨幣名稱');
                $table->decimal('deposit_rate', 15, 10)->default(1.0000000000)->comment('匯率');
                $table->decimal('withdrawal_rate', 15, 10)->default(1.0000000000)->comment('提款匯率');
                $table->tinyInteger('support_deposit')->default(0)->comment('支援存款');
                $table->tinyInteger('support_withdrawal')->default(0)->comment('支援提款');
                $table->tinyInteger('support_mining')->default(0)->comment('支援挖礦');
                $table->tinyInteger('status')->default(0)->comment('狀態: 0:disable, 1:enable, 9:block');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'organization_id' => 1, 'type' => 1, 'code' => 'CODE', 'name' => '平台幣', 'deposit_rate' => 1.0000000000, 'withdrawal_rate' => 1.0000000000, 'support_deposit' => 0, 'support_withdrawal' => 0, 'support_mining' => 0, 'status' => 1],
                ['id' => 11, 'organization_id' => 1, 'type' => 2, 'code' => 'USD', 'name' => 'USD', 'deposit_rate' => 1.0000000000, 'withdrawal_rate' => 1.3000000000, 'support_deposit' => 1, 'support_withdrawal' => 0, 'support_mining' => 0, 'status' => 1],
                ['id' => 12, 'organization_id' => 1, 'type' => 2, 'code' => 'MYR', 'name' => 'MYR', 'deposit_rate' => 0.2317430000, 'withdrawal_rate' => 4.3692000000, 'support_deposit' => 1, 'support_withdrawal' => 1, 'support_mining' => 0, 'status' => 1],
                ['id' => 13, 'organization_id' => 1, 'type' => 2, 'code' => 'IDR', 'name' => 'IDR', 'deposit_rate' => 0.0000650000, 'withdrawal_rate' => 15452.8000000000, 'support_deposit' => 1, 'support_withdrawal' => 0, 'support_mining' => 0, 'status' => 1],
                ['id' => 14, 'organization_id' => 1, 'type' => 2, 'code' => 'THB', 'name' => 'THB', 'deposit_rate' => 0.0291000000, 'withdrawal_rate' => 33.9000000000, 'support_deposit' => 1, 'support_withdrawal' => 0, 'support_mining' => 0, 'status' => 1],
                ['id' => 15, 'organization_id' => 1, 'type' => 2, 'code' => 'VND', 'name' => 'VND', 'deposit_rate' => 0.0000400000, 'withdrawal_rate' => 24695.0200000000, 'support_deposit' => 1, 'support_withdrawal' => 0, 'support_mining' => 0, 'status' => 1],
                ['id' => 101, 'organization_id' => 1, 'type' => 3, 'code' => 'USDT', 'name' => 'USDT', 'deposit_rate' => 1.0000000000, 'withdrawal_rate' => 1.0000000000, 'support_deposit' => 1, 'support_withdrawal' => 1, 'support_mining' => 0, 'status' => 1],
                ['id' => 102, 'organization_id' => 1, 'type' => 3, 'code' => 'BTC', 'name' => 'BTC', 'deposit_rate' => 0.0000170000, 'withdrawal_rate' => 0.0000170000, 'support_deposit' => 0, 'support_withdrawal' => 0, 'support_mining' => 1, 'status' => 1],
                ['id' => 103, 'organization_id' => 1, 'type' => 3, 'code' => 'ETH', 'name' => 'ETH', 'deposit_rate' => 0.0003993000, 'withdrawal_rate' => 0.0003959389, 'support_deposit' => 0, 'support_withdrawal' => 0, 'support_mining' => 0, 'status' => 1],
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
