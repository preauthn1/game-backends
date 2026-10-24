import './pagination.dart';

class FundRecord {
  final List<FundRecordItem>? list;
  final Pagination? pagination;

  FundRecord({
    this.list,
    this.pagination,
  });

  factory FundRecord.fromJson(Map<String, dynamic> json) {
    return FundRecord(
      list: json['list'] != null
          ? (json['list'] as List)
              .map((i) => FundRecordItem.fromJson(i))
              .toList()
          : null,
      pagination: json['pagination'] != null
          ? Pagination.fromJson(json['pagination'])
          : null,
    );
  }
}

class FundRecordItem {
  final String? id;
  final String? walletTypeId;
  final String? walletTypeName;
  final String? transactionTypeId;
  final String? transactionTypeName;
  final String? point;
  final String? beforeBalance;
  final String? afterBalance;
  final String? remark;
  final String? date;
  final String? createdAt;
  final String? operator;
  final String? status;

  FundRecordItem({
    this.id,
    this.walletTypeId,
    this.walletTypeName,
    this.transactionTypeId,
    this.transactionTypeName,
    this.point,
    this.beforeBalance,
    this.afterBalance,
    this.remark,
    this.date,
    this.createdAt,
    this.operator,
    this.status,
  });

  factory FundRecordItem.fromJson(Map<String, dynamic> json) {
    return FundRecordItem(
      id: json['id']?.toString(),
      walletTypeId: json['wallet_type_id']?.toString(),
      walletTypeName: json['wallet_type_name']?.toString(),
      transactionTypeId: json['transaction_type_id']?.toString(),
      transactionTypeName: json['transaction_type_name']?.toString(),
      point: json['point']?.toString(),
      beforeBalance: json['before_balance']?.toString(),
      afterBalance: json['after_balance']?.toString(),
      remark: json['remark']?.toString(),
      date: json['date']?.toString(),
      createdAt: json['created_at']?.toString(),
      operator: json['operator']?.toString(),
      status: json['status']?.toString(),
    );
  }
}

class FundRecordType {
  final String? id;
  final String? name;

  FundRecordType({
    this.id,
    this.name,
  });

  factory FundRecordType.fromJson(Map<String, dynamic> json) {
    return FundRecordType(
      id: json['id']?.toString(),
      name: json['name']?.toString(),
    );
  }
}
