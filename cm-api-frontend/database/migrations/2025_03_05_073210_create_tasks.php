<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration
{
    protected string $table_name = 'tasks';
    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->string('command', 150)->collation('utf8mb4_unicode_ci')->comment('命令');
                $table->string('expression', 15)->collation('utf8mb4_unicode_ci')->comment('時間表達式');
                $table->string('description', 500)->collation('utf8mb4_unicode_ci')->nullable()->default('')->comment('描述');
                $table->tinyInteger('status')->default(0)->comment('狀態 0=禁用; 1=啟用;');
                $table->text('info')->collation('utf8mb4_unicode_ci')->comment('其他資訊');
                $table->integer('operator')->default(0)->comment('最後操作者(agent.id)');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');
                $table->timestamp('last_run_at')->nullable()->comment('最後執行時間');

                //unique
                $table->unique('command', 'uni_' . $this->table_name . '_' . 'command');
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
