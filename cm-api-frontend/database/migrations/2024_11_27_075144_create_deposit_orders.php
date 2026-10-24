<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'deposit_orders';

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
                $table->string('bank_account_id', 45)->collation('utf8mb4_unicode_ci')->default('0');
                $table->integer('member_id')->comment('帳號系統編號(User)');
                $table->decimal('amount', 30, 10)->comment('充值金額');
                $table->decimal('rate', 12, 6)->default(1.000000)->comment('當下匯率');
                $table->decimal('point', 30, 10)->comment('平台點數');
                $table->string('cardholder', 100)->collation('utf8mb4_unicode_ci')->nullable()->comment('持卡人');
                $table->text('image')->collation('utf8mb4_unicode_ci')->comment('訂單收據');
                $table->string('transaction_no', 200)->collation('utf8mb4_unicode_ci')->comment('交易序號');
                $table->bigInteger('ip')->default(0)->comment('玩家IP地址');
                $table->tinyInteger('status')->default(0)->comment('狀態 0:建立,1:成功(已上分),2:失敗,3:待審核');
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
                $table->unique('order_no', 'uni_' . $this->table_name . '_' . 'order_no');
                //index
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
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
