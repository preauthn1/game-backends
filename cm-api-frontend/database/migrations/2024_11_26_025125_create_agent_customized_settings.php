<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'agent_customized_settings';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('PK流水號');
                $table->integer('agent_group_id')->comment('設置分組ID');
                $table->integer('setting_key')->comment('主要分類鍵值 (請參考 AgentCustomizedSettingKeyEnum)');
                $table->string('sub_category', 50)->comment('子分類 (同一組key、可能有多種設置 e.g. 語系)');
                $table->text('value')->comment('設值數值');
                $table->string('memo', 191)->comment('備註');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('agent_group_id', 'idx_' . $this->table_name . '_' . 'agent_group_id');
                $table->index('setting_key', 'idx_' . $this->table_name . '_' . 'setting_key');
                $table->index('sub_category', 'idx_' . $this->table_name . '_' . 'sub_category');
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
