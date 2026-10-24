<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'sessions';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->string('id', 191)->collation('utf8mb4_unicode_ci');
                $table->unsignedBigInteger('agent_id')->nullable();
                $table->string('ip_address', 45)->collation('utf8mb4_unicode_ci')->nullable();
                $table->text('user_agent')->collation('utf8mb4_unicode_ci');
                $table->longText('payload')->collation('utf8mb4_unicode_ci');
                $table->integer('last_activity');

                //index
                $table->index('agent_id', 'idx_' . $this->table_name . '_' . 'agent_id');
                $table->index('last_activity', 'idx_' . $this->table_name . '_' . 'last_activity_index');
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
