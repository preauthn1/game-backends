<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'agent_personal_settings';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->integer('agent_id')->comment('代理id');
                $table->integer('setting_id')->comment('設置鍵值');
                $table->string('sub_category', 50)->collation('utf8mb4_unicode_ci')->nullable()->comment('子分類(可能是語系)');
                $table->string('value', 200)->collation('utf8mb4_unicode_ci')->comment('設置value');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->integer('created_by')->comment('建立人');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');
                $table->integer('admin')->default(0)->comment('操作者');

                //index
                $table->index('agent_id', 'idx_' . $this->table_name . '_' . 'agent_id');
                $table->index('setting_id', 'idx_' . $this->table_name . '_' . 'setting_id');
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
