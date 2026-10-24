import 'package:flutter/material.dart';

import 'base_card.dart';

class BaseDropdown extends StatelessWidget {
  final List<DropdownMenuItem> items;
  final void Function(dynamic)? onChanged;
  final dynamic value;
  final double borderRadius;
  final String? labelText;
  final bool isExpanded;
  final EdgeInsetsGeometry padding;

  const BaseDropdown({
    required this.items,
    required this.onChanged,
    this.value,
    this.labelText,
    super.key,
  })  : borderRadius = 10,
        isExpanded = true,
        padding = const EdgeInsets.symmetric(horizontal: 14);

  const BaseDropdown.small({
    required this.items,
    required this.onChanged,
    this.value,
    super.key,
  })  : borderRadius = 25,
        isExpanded = false,
        padding = const EdgeInsets.symmetric(horizontal: 14),
        labelText = null;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        if (labelText != null) Text(labelText!),
        if (labelText != null) const SizedBox(height: 4),
        BaseCard(
          borderRadius: borderRadius,
          child: DropdownButton(
            padding: padding,
            value: value,
            onChanged: onChanged,
            borderRadius: BorderRadius.circular(borderRadius),
            underline: const SizedBox(),
            dropdownColor: const Color(0xFF18171C),
            isExpanded: isExpanded,
            items: items,
          ),
        ),
      ],
    );
  }
}
