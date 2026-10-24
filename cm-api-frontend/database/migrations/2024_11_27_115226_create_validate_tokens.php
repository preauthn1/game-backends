<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'validate_tokens';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->integer('member_id')->comment('會員系統編號(members)');
                $table->tinyInteger('type_id')->comment('類型(十位數-1:資料驗證, 2:忘記密碼, 3:找回密碼, 4:洗分驗證, 5:支付密碼, 6:忘記支付密碼, 7:洗分驗證, 8:一頁註冊，個位數-1:手機2:Email)');
                $table->string('mobile', 80)->collation('utf8mb4_unicode_ci')->comment('mobile');
                $table->text('info')->collation('utf8mb4_unicode_ci')->comment('其他資訊');
                $table->string('token', 80)->collation('utf8mb4_unicode_ci')->comment('Token');
                $table->dateTime('expire_date')->comment('過期日期');
                $table->tinyInteger('status')->comment('狀態');
                $table->integer('times')->default(1)->comment('發送次數');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
                $table->index('type_id', 'idx_' . $this->table_name . '_' . 'type_id');
                $table->index('mobile', 'idx_' . $this->table_name . '_' . 'mobile');
                $table->index('expire_date', 'idx_' . $this->table_name . '_' . 'expire_date');
                $table->index('token', 'idx_' . $this->table_name . '_' . 'token');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
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
