<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;

return new class extends Migration
{
    protected string $table_name = 'interval_daily_reports';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->date('summary_date')->comment('彙整日期');
                $table->integer('agent_id')->comment('代理系統編號(agent)');
                //會員數據說明
                $table->tinyInteger('regi_count')->default(0)->comment('註冊數; 當日完成註冊的新會員數量');
                $table->tinyInteger('login_count')->default(0)->comment('登入數; 24小時內登入系統的不重複會員數量');
                $table->tinyInteger('active_count')->default(0)->comment('活躍會員數; 當日有充值操作或提現操作或查看收益的會員數量');
                //資金數據說明
                $table->decimal('deposit_amount', 30, 10)->default(0.0000000000)->comment('充值總額; 當日成功充值的總金額');
                $table->decimal('withdrawal_amount', 30, 10)->default(0.0000000000)->comment('提現總額; 當日成功提現的總金額');
                //消費數據說明
                $table->decimal('purchase_amount', 30, 10)->default(0.0000000000)->comment('購買(團購)總額; 當日礦機購買訂單的總金額');
                $table->tinyInteger('purchase_count')->default(0)->comment('購買(團購)數量; 當日礦機購買訂單的總數量');
                $table->decimal('rent_amount', 30, 10)->default(0.0000000000)->comment('租賃總額; 當日礦機租賃訂單的總金額');
                $table->tinyInteger('rent_count')->default(0)->comment('租賃數量; 當日礦機租賃訂單的總數量');
                // 收益數據說明
                $table->decimal('purchase_income', 30, 10)->default(0.0000000000)->comment('購買礦機收益; 會員購買礦機產生的收益');
                $table->decimal('group_income', 30, 10)->default(0.0000000000)->comment('團購礦機收益; 會員團購礦機產生的收益');
                $table->decimal('rent_income', 30, 10)->default(0.0000000000)->comment('租賃礦機收益; 會員租用礦機產生的收益');
                $table->decimal('reward_income', 30, 10)->default(0.0000000000)->comment('邀請獎勵; 當日產生的邀請新會員獎勵總額');
                $table->decimal('rebate_income', 30, 10)->default(0.0000000000)->comment('返佣收益：當日產生的下級會員消費返佣總額');
                $table->decimal('award_amount', 30, 10)->default(0.0000000000)->comment('獎勵總額; 人工獎勵的總金額');
                $table->string('memo', 300)->collation('utf8mb4_unicode_ci')->nullable()->comment('備註');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //unique
                $table->unique(['summary_date', 'agent_id'], 'uni_' . $this->table_name . '_1');
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
