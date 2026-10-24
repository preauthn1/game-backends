import './pagination.dart';

class WithdrawalRecord {
  final List<WithdrawalRecordItem>? list;
  final Pagination? pagination;

  WithdrawalRecord({
    this.list,
    this.pagination,
  });

  factory WithdrawalRecord.fromJson(Map<String, dynamic> json) {
    return WithdrawalRecord(
      list: json['list'] != null
          ? (json['list'] as List)
              .map((i) => WithdrawalRecordItem.fromJson(i))
              .toList()
          : null,
      pagination: json['pagination'] != null
          ? Pagination.fromJson(json['pagination'])
          : null,
    );
  }
}

class WithdrawalRecordItem {
  final String? id;
  final String? orderNo;
  final String? bankId;
  final String? memberId;
  final String? amount;
  final String? exchangeRate;
  final String? point;
  final String? cardholder;
  final String? ip;
  final String? status;
  final String? extraContent;
  final String? remark;
  final String? payTime;
  final String? operator;
  final String? createdAt;
  final String? updatedAt;
  final String? bankName;
  final String? bankType;
  final String? currencyId;
  final String? currencyName;
  final String? afterBalance;

  WithdrawalRecordItem({
    this.id,
    this.orderNo,
    this.bankId,
    this.memberId,
    this.amount,
    this.exchangeRate,
    this.point,
    this.cardholder,
    this.ip,
    this.status,
    this.extraContent,
    this.remark,
    this.payTime,
    this.operator,
    this.createdAt,
    this.updatedAt,
    this.bankName,
    this.bankType,
    this.currencyId,
    this.currencyName,
    this.afterBalance,
  });

  factory WithdrawalRecordItem.fromJson(Map<String, dynamic> json) {
    return WithdrawalRecordItem(
      id: json['id']?.toString(),
      orderNo: json['order_no']?.toString(),
      bankId: json['bank_id']?.toString(),
      memberId: json['member_id']?.toString(),
      amount: json['amount']?.toString(),
      exchangeRate: json['exchange_rate']?.toString(),
      point: json['point']?.toString(),
      cardholder: json['cardholder']?.toString(),
      ip: json['ip']?.toString(),
      status: json['status']?.toString(),
      extraContent: json['extra_content']?.toString(),
      remark: json['remark']?.toString(),
      payTime: json['pay_time']?.toString(),
      operator: json['operator']?.toString(),
      createdAt: json['created_at']?.toString(),
      updatedAt: json['updated_at']?.toString(),
      bankName: json['bank_name']?.toString(),
      bankType: json['bank_type']?.toString(),
      currencyId: json['currency_id']?.toString(),
      currencyName: json['currency_name']?.toString(),
      afterBalance: json['after_balance']?.toString(),
    );
  }
}
