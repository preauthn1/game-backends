<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'member_bank_accounts';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->integer('member_id')->comment('會員系統編號(members.id)');
                $table->integer('bank_id')->default(0)->comment('銀行系統編號(banks.id)');
                $table->string('account_name', 50)->collation('utf8mb4_unicode_ci')->comment('帳號用戶名');
                $table->string('account_no', 50)->collation('utf8mb4_unicode_ci')->comment('帳號');
                $table->integer('sort')->default(0)->comment('排序');
                $table->tinyInteger('status')->default(0)->comment('狀態: 0:disable, 1:enable, 9:block');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
                $table->index('bank_id', 'idx_' . $this->table_name . '_' . 'bank_id');
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
