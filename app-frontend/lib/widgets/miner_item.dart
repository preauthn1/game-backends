import 'package:flutter/material.dart';

import '../models/purchase_miner.dart';
import '../models/rent_miner.dart';
import '../utils/utils.dart';
import './base_card.dart';

class MinerItem extends StatelessWidget {
  final PurchaseMiner? purchaseMiner;
  final RentMiner? rentMiner;
  final Function(PurchaseMiner miner)? onTapPurchase;
  final Function(RentMiner miner)? onTapRent;

  const MinerItem.purchase(
    this.purchaseMiner, {
    this.onTapPurchase,
    super.key,
  })  : rentMiner = null,
        onTapRent = null;

  const MinerItem.rent(
    this.rentMiner, {
    this.onTapRent,
    super.key,
  })  : purchaseMiner = null,
        onTapPurchase = null;

  String? get image => purchaseMiner?.image ?? rentMiner?.image;
  String? get tag => purchaseMiner?.tag ?? rentMiner?.tag;
  String? get tagColor => purchaseMiner?.tagColor ?? rentMiner?.tagColor;
  String? get name => purchaseMiner?.name ?? rentMiner?.name;
  String? get price => purchaseMiner?.purchasePrice ?? rentMiner?.rentPrice;

  @override
  Widget build(BuildContext context) {
    return BaseCard(
      onTap: onTapPurchase != null && purchaseMiner != null
          ? () => onTapPurchase!(purchaseMiner!)
          : onTapRent != null && rentMiner != null
              ? () => onTapRent!(rentMiner!)
              : null,
      child: Padding(
        padding: const EdgeInsets.all(12),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            SizedBox(
              width: double.infinity,
              height: 162,
              child: image != null
                  ? Image.network(image!, fit: BoxFit.contain)
                  : null,
            ),
            const SizedBox(height: 12),
            if (tag != null)
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 2),
                decoration: BoxDecoration(
                  color: hexToColor(tagColor ?? '#FFFFFF'),
                  borderRadius: BorderRadius.circular(4),
                ),
                child: Text(
                  tag!,
                  style: const TextStyle(
                    color: Colors.black,
                    fontSize: 10,
                  ),
                ),
              ),
            if (tag == null) const SizedBox(height: 18),
            Text(name ?? ''),
            const SizedBox(height: 8),
            Text(
              '${formatNumber(price)} USDT',
              style: const TextStyle(fontWeight: FontWeight.w700),
            ),
          ],
        ),
      ),
    );
  }
}
