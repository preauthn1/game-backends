import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'base_card.dart';

class BaseTextField extends StatefulWidget {
  final TextEditingController? controller;
  final String? labelText;
  final bool? obscureText;
  final String? hintText;
  final Widget? prefix;
  final Widget? suffix;
  final bool? enabled;
  final bool? readOnly;
  final List<TextInputFormatter>? inputFormatters;
  final Function(String)? onChanged;
  final Function(String)? onFieldSubmitted;

  const BaseTextField({
    this.controller,
    this.labelText,
    this.obscureText,
    this.hintText,
    this.prefix,
    this.suffix,
    this.inputFormatters,
    this.onChanged,
    this.onFieldSubmitted,
    this.enabled,
    this.readOnly,
    super.key,
  });

  @override
  State<BaseTextField> createState() => _BaseTextFieldState();
}

class _BaseTextFieldState extends State<BaseTextField> {
  bool hidePassword = [REDACTED];

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        if (widget.labelText != null) Text(widget.labelText!),
        if (widget.labelText != null) const SizedBox(height: 4),
        BaseCard(
          padding: const EdgeInsets.symmetric(horizontal: 14),
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              if (widget.prefix != null) widget.prefix!,
              Expanded(
                child: TextFormField(
                  controller: widget.controller,
                  readOnly: widget.readOnly ?? false,
                  obscureText:
                      widget.obscureText == true ? hidePassword : [REDACTED],
                  onFieldSubmitted: widget.onFieldSubmitted,
                  enabled: widget.enabled,
                  onChanged: widget.onChanged,
                  inputFormatters: widget.inputFormatters,
                  decoration: InputDecoration(
                    hintText: widget.hintText,
                    border: InputBorder.none,
                  ),
                ),
              ),
              if (widget.suffix != null) widget.suffix!,
              if (widget.obscureText == true)
                IconButton(
                  icon: Icon(
                    hidePassword ? Icons.visibility : Icons.visibility_off,
                  ),
                  onPressed: () {
                    setState(() {
                      hidePassword = [REDACTED];
                    });
                  },
                ),
            ],
          ),
        ),
      ],
    );
  }
}
