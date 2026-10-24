<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'member_access_tokens';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->integer('member_id')->comment('會員帳號系統編號(Members.id)');
                $table->string('token', 80)->collation('utf8mb4_unicode_ci')->comment('Token');
                $table->dateTime('expire_date')->comment('過期日期');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('token', 'idx_' . $this->table_name . '_' . 'token');
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
                $table->index('expire_date', 'idx_' . $this->table_name . '_' . 'expire_date');
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
