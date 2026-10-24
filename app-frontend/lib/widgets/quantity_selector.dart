import 'package:flutter/material.dart';

import 'base_card.dart';

class QuantitySelector extends StatelessWidget {
  final int quantity;
  final int? min;
  final int? max;
  final Function(int)? onChanged;

  const QuantitySelector({
    super.key,
    this.quantity = 1,
    this.min,
    this.max,
    this.onChanged,
  });

  @override
  Widget build(BuildContext context) {
    final bool canDecrement =
        onChanged != null && (min == null || quantity > min!);
    final bool canIncrement =
        onChanged != null && (max == null || quantity < max!);

    return BaseCard(
      child: Row(
        children: [
          InkWell(
            borderRadius: const BorderRadius.only(
              topLeft: Radius.circular(10),
              bottomLeft: Radius.circular(10),
            ),
            onTap: canDecrement ? () => onChanged!(quantity - 1) : null,
            child: SizedBox(
              width: 30,
              height: 30,
              child: Icon(
                Icons.remove,
                color: canDecrement ? null : Colors.grey,
              ),
            ),
          ),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 12),
            decoration: const BoxDecoration(
              border: Border.symmetric(
                vertical: BorderSide(
                  color: Color(0xFF252525),
                ),
              ),
            ),
            child: Text(
              '$quantity',
              style: const TextStyle(
                fontSize: 16,
              ),
            ),
          ),
          InkWell(
            borderRadius: const BorderRadius.only(
              topRight: Radius.circular(10),
              bottomRight: Radius.circular(10),
            ),
            onTap: canIncrement ? () => onChanged!(quantity + 1) : null,
            child: SizedBox(
              width: 30,
              height: 30,
              child: Icon(
                Icons.add,
                color: canIncrement ? null : Colors.grey,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
