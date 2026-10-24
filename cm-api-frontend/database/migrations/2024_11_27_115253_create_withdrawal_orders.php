<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'withdrawal_orders';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->string('order_no', 32)->collation('utf8mb4_unicode_ci')->comment('訂單編號');
                $table->integer('bank_id')->default(0)->comment('銀行資訊編號(banks.id)');
                $table->integer('member_id')->comment('帳號系統編號(User)');
                $table->decimal('point', 30, 10)->comment('平台點數');
                $table->decimal('fee', 30, 10)->default(0.0000000000)->comment('手續費');
                $table->decimal('rate', 12, 6)->default(1.000000)->comment('當下匯率');
                $table->decimal('amount', 30, 10)->comment('提領金額');
                $table->string('cardholder', 100)->collation('utf8mb4_unicode_ci')->nullable()->comment('持卡人');
                $table->string('cardno', 100)->collation('utf8mb4_unicode_ci')->nullable()->comment('銀行卡號/Crypto address');
                $table->string('ip', 50)->collation('utf8mb4_unicode_ci')->default('')->comment('玩家IP地址');
                $table->tinyInteger('status')->default(0)->comment('狀態 0:建立,1:成功,2:取消並退回,3:審核通過,4:取消不退回');
                $table->text('extra_content')->collation('utf8mb4_unicode_ci')->comment('訂單額外內容');
                $table->text('remark')->collation('utf8mb4_unicode_ci')->comment('備註');
                $table->dateTime('pay_time')->nullable()->comment('支付時間');
                $table->integer('operator')->default(0)->comment('操作者');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //unique
                $table->unique('order_no', 'uni_withdrawal_orders_order_no');
                //index
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
                $table->index('bank_id', 'idx_' . $this->table_name . '_' . 'banks_id');
                $table->index('pay_time', 'idx_' . $this->table_name . '_' . 'pay_time');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
                $table->index('created_at', 'idx_' . $this->table_name . '_' . 'created_at');
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
