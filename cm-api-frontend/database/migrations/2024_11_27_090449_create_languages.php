<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'languages';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->string('name', 32)->collation('utf8mb4_unicode_ci')->comment('語言名稱');
                $table->string('code', 10)->collation('utf8mb4_unicode_ci')->comment('語言代碼');
                $table->tinyInteger('sort')->default(0)->comment('排序');
                $table->tinyInteger('status')->default(0)->comment('狀態 0:停用,1:啟用');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'name' => 'English', 'code' => 'en', 'status' => 1],
                ['id' => 2, 'name' => '繁體中文', 'code' => 'zh-tw', 'status' => 1],
                ['id' => 3, 'name' => '简体中文', 'code' => 'zh-cn', 'status' => 1],
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
