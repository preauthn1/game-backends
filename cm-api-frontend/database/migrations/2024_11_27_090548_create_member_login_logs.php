<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'member_login_logs';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('member_id')->comment('會員帳號系統編號(Member)');
                $table->bigInteger('ip')->nullable()->comment('登ip');
                $table->string('ipv6', 50)->collation('utf8mb4_unicode_ci')->default('')->comment('IPv6');
                $table->text('trace_ip')->collation('utf8mb4_unicode_ci')->comment('完整IP地址');
                $table->tinyInteger('status')->default(0)->comment('狀態：0失敗，1成功');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('created_at', 'idx_' . $this->table_name . '_' . '_created_at');
                $table->index('ip', 'idx_' . $this->table_name . '_' . '_ip');
                $table->index('member_id', 'idx_' . $this->table_name . '_' . '_member_id');
                $table->index('status', 'idx_' . $this->table_name . '_' . '_status');
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
