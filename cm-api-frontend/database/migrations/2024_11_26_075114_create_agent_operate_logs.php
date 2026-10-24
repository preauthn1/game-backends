<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'agent_operate_logs';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->string('api_path', 500)->collation('utf8mb4_unicode_ci');
                $table->string('api_name', 100)->collation('utf8mb4_unicode_ci')->comment('異動Model');
                $table->text('request')->collation('utf8mb4_unicode_ci')->comment('異動前資料');
                $table->integer('response_code');
                $table->string('response_message', 500)->collation('utf8mb4_unicode_ci');
                $table->text('response_data')->collation('utf8mb4_unicode_ci')->comment('異動後資料');
                $table->decimal('time_cost', 10, 4)->default(0.0000);
                $table->tinyInteger('type')->default(0)->comment('類型 1=成功; 2=exception;');
                $table->integer('agent_id')->comment('代理身份證系統編號(agents.id)');
                $table->bigInteger('agent_ip')->comment('代理IP');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('api_name', 'idx_' . $this->table_name . '_' . 'api_name');
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
