<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'bank_accounts';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->integer('bank_id')->comment('銀行id');
                $table->string('account_name', 191)->collation('utf8mb4_unicode_ci')->comment('帳戶名稱');
                $table->string('account_nickname', 50)->collation('utf8mb4_unicode_ci')->comment('帳戶簡稱');
                $table->string('account_no', 100)->collation('utf8mb4_unicode_ci')->comment('銀行帳號');
                $table->string('remark', 500)->collation('utf8mb4_unicode_ci')->nullable()->comment('備註');
                $table->tinyInteger('status')->default(1)->comment('status; 0:disable, 1:enable');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('bank_id', 'idx_' . $this->table_name . '_' . 'bank_id');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'bank_id' => 3, 'account_name' => 'MYR', 'account_nickname' => 'MYR-1', 'account_no' => '1245000001'],
                ['id' => 2, 'bank_id' => 1, 'account_name' => 'TRC-1', 'account_nickname' => 'TRC-1', 'account_no' => '12300001'],
                ['id' => 3, 'bank_id' => 2, 'account_name' => 'ERC-1', 'account_nickname' => 'ERC-1', 'account_no' => '1230001'],
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
