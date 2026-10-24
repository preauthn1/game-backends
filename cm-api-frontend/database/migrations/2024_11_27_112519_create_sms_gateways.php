<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'sms_gateways';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->string('name', 45)->collation('utf8mb4_unicode_ci')->comment('簡訊商名稱');
                $table->string('code', 10)->collation('utf8mb4_unicode_ci')->comment('簡訊商代碼');
                $table->string('info', 200)->collation('utf8mb4_unicode_ci')->default('{}')->comment('簡訊商輸入資訊');
                $table->string('url', 200)->collation('utf8mb4_unicode_ci')->comment('簡訊商 API URL');
                $table->string('memo', 200)->collation('utf8mb4_unicode_ci')->comment('備註');
                $table->integer('status')->default(0)->comment('狀態; 0:停用, 1:啟用');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'name' => 'LOCAL', 'code' => 'LOCAL', 'info' => '{}', 'url' => '', 'memo' => '內部直接通過', 'status' => 0],
                ['id' => 2, 'name' => 'huanxun58', 'code' => 'huanxun58', 'info' => '{"SpCode":{"type":"text"},"LoginName":{"type":"text"},"Password":{"type":"text"},"insert_characters":{"type":"text"}}', 'url' => 'https://example.invalid', 'memo' => '', 'status' => 1],
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
