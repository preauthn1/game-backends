<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'miner_rebates';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('miner_id')->default(0)->comment('礦機資訊編號(miners.id)');
                $table->decimal('percentage1', 5, 4)->default(0.0000)->comment('自身回水百分比; 1%=0.01');
                $table->decimal('percentage2', 5, 4)->default(0.0000)->comment('上級回水百分比; 1%=0.01');
                $table->decimal('percentage3', 5, 4)->default(0.0000)->comment('上上級回水百分比; 1%=0.01');
                $table->tinyInteger('status')->default(1)->comment('狀態: 0:disable, 1:enable, 9:block');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('miner_id', 'idx_' . $this->table_name . '_' . 'miner_id');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
            });

            DB::table($this->table_name)->insert([
                ['miner_id' => 1, 'percentage1' => 0.0000, 'percentage2' => 0.0200, 'percentage3' => 0.0100, 'status' => 1],
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
