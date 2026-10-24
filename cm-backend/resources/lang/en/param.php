<?php
return [
    'member.type.1' => 'Real member',
    'member.type.2' => 'Internal tester',
    'member.type.3' => 'External tester',
    'member.type.9' => 'Special account',

    'member.status.1' => 'Normal',
    'member.status.2' => 'Password Lock',
    'member.status.9' => 'Blocked',

    'member_id_card.status.0' => 'Pending review',
    'member_id_card.status.1' => 'Verified',
    'member_id_card.status.2' => 'Review failed',
    'member_id_card.status.9' => 'Not real name',

    'member_child.level' => 'Level ',
    'member_child.level.0' => 'Level 0',
    'member_child.level.1' => 'Level 1',
    'member_child.level.2' => 'Level 2',

    'miner.type.0' => 'Rental',
    'miner.type.1' => 'Purchase',
    'miner.type.2' => 'Group',

    'deposit_order.status.0' => 'Created',
    'deposit_order.status.1' => 'Succeeded',
    'deposit_order.status.2' => 'Fail',
    'deposit_order.status.3' => 'Waiting Review',

    'deposit_gateway.1001' => 'Manual Recharge - Bank',
    'deposit_gateway.1002' => 'Manual Recharge - Cryptocurrency',

    'currency.type.1' => 'Platform Token',
    'currency.type.2' => 'Fiat Currency',
    'currency.type.3' => 'Cryptocurrency',

    'withdrawal_order.status.0' => 'Created',
    'withdrawal_order.status.1' => 'Approved',
    'withdrawal_order.status.2' => 'Canceled Then Back',
    'withdrawal_order.status.3' => 'Pending',
    'withdrawal_order.status.4' => 'Canceled Not Back',

    'withdrawal_gateway.1001' => 'Manual Withdrawal - Bank',
    'withdrawal_gateway.1002' => 'Manual Withdrawal - Cryptocurrency',

    'withdrawal_fee.type.0' => 'Amount Rate',
    'withdrawal_fee.type.1' => 'Fixed Amount',

    'purchase_order.status.0' => 'Created',
    'purchase_order.status.1' => 'In progress',
    'purchase_order.status.2' => 'Ended',

    'rent_order.status.0' => 'Pending',
    'rent_order.status.1' => 'In progress',
    'rent_order.status.2' => 'Ended',

    'group_order.status.0' => 'Group purchase order creation',
    'group_order.status.1' => 'In progress',
    'group_order.status.2' => 'Canceled',
    'group_order.status.3' => 'Canceling',

    'transaction_type.1' => 'Deposit',
    'transaction_type.2' => 'Withdrawal',
    'transaction_type.3' => 'Purchase Mining',
    'transaction_type.4' => 'Rental Mining',
    'transaction_type.5' => 'Cancel Group Mining',
    'transaction_type.6' => 'Sell Mining',
    'transaction_type.7' => 'Mining Profits',
    'transaction_type.8' => 'Group Mining Profits',
    'transaction_type.9' => 'Rental Mining Profits',
    'transaction_type.10' => 'Referral Rewards',
    'transaction_type.11' => 'Sales Rebate',
    'transaction_type.12' => 'Automatic increase',
    'transaction_type.13' => 'Manual Withdrawal',
    'transaction_type.14' => 'Refund the withdrawal amount',
    'transaction_type.15' => 'Registration Reward',
    'transaction_type.16' => 'Promotion',
    'transaction_type.17' => 'Sub Balance',
    'transaction_type.18' => 'Commission',

    'announcement.type.1' => 'System',
    'announcement.type.2' => 'Promotion',
    'announcement.type.3' => 'Platform',

    'promotion.type.0' => 'Open URL',
    'promotion.type.1' => 'Login Page Popup',
    'promotion.type.2' => 'Inner Page Popup',

    'profit_analysis.orderBy.1' => 'Deposit',
    'profit_analysis.orderBy.2' => 'Withdrawal',
    'profit_analysis.orderBy.3' => 'Purchase',
    'profit_analysis.orderBy.4' => 'Rebate',
    'profit_analysis.orderBy.5' => 'Reward',
    'profit_analysis.orderBy.6' => 'Earning',
    'profit_analysis.orderBy.7' => 'Depreciation',
    'profit_analysis.orderBy.8' => 'Total Profit',

    'profit_analysis.sort.1' => 'Increase',
    'profit_analysis.sort.2' => 'Decrease',

    'agent_operate_type_action.change_password' => 'Reset password',
    'agent_operate_type_action.update_member' => 'Update Member Setting/Password',
    'agent_operate_type_action.update_agent' => 'Update Agent Setting/Password',
    'agent_operate_type_action.migrate_agent' => 'Migrate agent',
    'agent_operate_type_action.agent_login' => 'Agent Login',
    'agent_operate_type_action.agent_login_fail' => 'Agent Login Fail',
    'agent_operate_type_action.agent_permission_sync' => 'Sync Agent Permission',
    'agent_operate_type_action.order_manual_deposit' => 'Manual Deposit',
    'agent_operate_type_action.order_reset_status' => 'Order reset status',
    'agent_operate_type_action.cs_user_login' => 'CS backend login',
    'agent_operate_type_action.cs_user_login_fail' => 'CS backend login failed',
    'agent_operate_type_action.update_mobile_validate' => 'Modify phone authentication',
    'agent_operate_type_action.set_agent_payment_password' => 'Set Agent Payment Password',
    'agent_operate_type_action.modify_agent_payment_password' => 'Change Agent Payment Password',
    'agent_operate_type_action.reset_agent_payment_password' => 'Reset Agent Payment Password',

    'transaction_type.to_vendor' => 'platform transfer to games',
    'transaction_type.from_vendor' => 'game transfer to platform',
    'transaction_type.deposit' => 'topup',

    'advertisement_type.game_type' => 'types of game',
    'advertisement_type.game' => 'game',

    'prize_button.exchange' => 'redeem now',
    'prize_button.exchanged' => 'claimed',
    'prize_button.expired' => 'expired',

    'agent_group.type.1' => 'customized',
    'agent_group.type.2' => 'payment',
    'agent_group.type.3' => 'sms',
    'agent_group.type.4' => 'miner',

    'data_type.integer' => 'integer',
    'data_type.string' => 'string',
    'data_type.boolean' => 'boolean',
    'data_type.decimal' => 'decimal',

    'pass' => 'Pass',
    'no_pass' => 'Did not pass',

    'no_draw' => 'No draw',
    'unaccalimed' => 'Unaccalimed',
    'received' => 'Received',
    'did_not_win' => 'Did not win',

    'isExperience.0' => 'no',
    'isExperience.1' => 'yes',

    'success' => 'Success',
    'fail' => 'Fail',
    'system' => 'System',
    'customer_service' => 'Customer Service',
    'first_deposit' => 'First charge reward',
    'popup_text' => 'Popup text',
    'first_layer_agent' => 'first_layer_agent',
    'second_layer_agent' => 'second_layer_agent',
    'third_layer_agent' => 'third_layer_agent',
    'enable' => 'Enable',
    'disable' => 'Disable',
    'all' => 'All',
    'other' => 'other',
    'unknown' => 'unknown'
];
