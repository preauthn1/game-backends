import './pagination.dart';

class TopUpRecord {
  final List<TopUpRecordItem>? list;
  final Pagination? pagination;

  TopUpRecord({
    this.list,
    this.pagination,
  });

  factory TopUpRecord.fromJson(Map<String, dynamic> json) {
    return TopUpRecord(
      list: json['list'] != null
          ? (json['list'] as List)
              .map((i) => TopUpRecordItem.fromJson(i))
              .toList()
          : null,
      pagination: json['pagination'] != null
          ? Pagination.fromJson(json['pagination'])
          : null,
    );
  }
}

class TopUpRecordItem {
  final String? id;
  final String? orderNo;
  final String? bankId;
  final String? memberId;
  final String? amount;
  final String? exchangeRate;
  final String? point;
  final String? cardholder;
  final String? image;
  final String? ip;
  final String? status;
  final String? extraContent;
  final String? remark;
  final String? payTime;
  final String? operator;
  final String? createdAt;
  final String? updatedAt;
  final String? bankType;
  final String? currencyId;
  final String? currencyName;
  final String? afterBalance;

  TopUpRecordItem({
    this.id,
    this.orderNo,
    this.bankId,
    this.memberId,
    this.amount,
    this.exchangeRate,
    this.point,
    this.cardholder,
    this.image,
    this.ip,
    this.status,
    this.extraContent,
    this.remark,
    this.payTime,
    this.operator,
    this.createdAt,
    this.updatedAt,
    this.bankType,
    this.currencyId,
    this.currencyName,
    this.afterBalance,
  });

  factory TopUpRecordItem.fromJson(Map<String, dynamic> json) {
    return TopUpRecordItem(
      id: json['id']?.toString(),
      orderNo: json['order_no']?.toString(),
      bankId: json['bank_id']?.toString(),
      memberId: json['member_id']?.toString(),
      amount: json['amount']?.toString(),
      exchangeRate: json['exchange_rate']?.toString(),
      point: json['point']?.toString(),
      cardholder: json['cardholder']?.toString(),
      image: json['image']?.toString(),
      ip: json['ip']?.toString(),
      status: json['status']?.toString(),
      extraContent: json['extra_content']?.toString(),
      remark: json['remark']?.toString(),
      payTime: json['pay_time']?.toString(),
      operator: json['operator']?.toString(),
      createdAt: json['created_at']?.toString(),
      updatedAt: json['updated_at']?.toString(),
      bankType: json['bank_type']?.toString(),
      currencyId: json['currency_id']?.toString(),
      currencyName: json['currency_name']?.toString(),
      afterBalance: json['after_balance']?.toString(),
    );
  }
}
