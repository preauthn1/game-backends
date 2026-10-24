<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'wallet_types';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->tinyInteger('type')->default(0)->comment('類別, 1:平台幣, 2:法定貨幣, 3:加密貨幣, 9:其他');
                $table->string('code', 50)->collation('utf8mb4_unicode_ci')->nullable()->comment('錢包代碼');
                $table->string('name', 50)->collation('utf8mb4_unicode_ci')->nullable()->comment('錢包名稱');
                $table->text('info')->collation('utf8mb4_unicode_ci')->comment('錢包資訊');
                $table->tinyInteger('status')->default(0)->comment('狀態: 0:disable, 1:enable, 9:block');
                $table->string('memo', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('方便技術記錄用');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //unique
                $table->unique('code', 'uni_wallet_types_code');
                //index
                $table->index('status', 'idx_wallet_types_status');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'type' => 1, 'code' => 'CASH', 'name' => 'USDT', 'info' => '{}', 'status' => 1, 'memo' => '平台幣'],
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
