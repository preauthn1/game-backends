<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'model_operate_logs';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->string('model', 100)->collation('utf8mb4_unicode_ci')->comment('異動Model');
                $table->string('operation', 100)->collation('utf8mb4_unicode_ci')->comment('異動動作');
                $table->text('before_data')->collation('utf8mb4_unicode_ci')->comment('異動前資料');
                $table->text('after_data')->collation('utf8mb4_unicode_ci')->comment('異動後資料');
                $table->integer('agent_id')->comment('代理身份證系統編號(agents.id)');
                $table->bigInteger('agent_ip')->comment('代理IP');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('model', 'idx_' . $this->table_name . '_' . 'model');
                $table->index('operation', 'idx_' . $this->table_name . '_' . 'operation');
                $table->index('agent_id', 'idx_' . $this->table_name . '_' . 'agent_id');
                $table->index('agent_ip', 'idx_' . $this->table_name . '_' . 'agent_ip');
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
