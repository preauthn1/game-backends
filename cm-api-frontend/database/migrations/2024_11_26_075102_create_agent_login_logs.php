<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'agent_login_logs';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('agent_id')->comment('代理系統編號(agents.id)');
                $table->bigInteger('ip')->nullable()->comment('IP地址');
                $table->string('ipv6', 100)->collation('utf8mb4_unicode_ci')->default('')->comment('IPV6地址');
                $table->string('remark', 500)->collation('utf8mb4_unicode_ci')->nullable()->comment('其他資訊');
                $table->tinyInteger('status')->default(0)->comment('登入狀態：0:fail, 1:success');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('agent_id', 'idx_' . $this->table_name . '_' . 'agent_id');
                $table->index('ip', 'idx_' . $this->table_name . '_' . 'ip');
                $table->index('ipv6', 'idx_' . $this->table_name . '_' . 'ipv6');
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
