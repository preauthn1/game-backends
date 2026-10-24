<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'sms_send_records';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('sms_gateway_id')->default(0)->comment('簡訊商ID(sms_gateways.id)');
                $table->unsignedInteger('member_id')->default(0)->comment('玩家ID');
                $table->integer('country_id')->default(0)->comment('Country.id');
                $table->string('mobile', 20)->collation('utf8mb4_unicode_ci')->comment('發送號碼');
                $table->string('message_content', 500)->collation('utf8mb4_unicode_ci')->comment('簡訊發送內容');
                $table->string('transaction_id', 50)->collation('utf8mb4_unicode_ci')->nullable()->comment('簡訊商發送ID');
                $table->text('request_info')->collation('utf8mb4_unicode_ci')->comment('request info(json)');
                $table->text('response_info')->collation('utf8mb4_unicode_ci')->comment('response data(json)');
                $table->tinyInteger('status')->default(0)->comment('發送狀態; 0:未發送, 1:發送成功, 2:發送失敗');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('sms_gateway_id', 'idx_' . $this->table_name . '_' . 'sms_gateway_id');
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
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
