# AI

Simple console app with a prompt to parse the provided json file using AWS Bedrock (Cohere).
You will need a default aws profile created to run this app

The input format looks like the following

```json
{
  "EligibilityDetails": "- The person must be located in Ecuador or Argentina\n- Must be less than 18 years old\n- Must have enough money to sustain itself\n- Must be willing to relocate\n- Must has a bank account in the XYZ Bank and in the WWW Bank\n- Can be graduated from any university except: ABC University and CBA University"
}
```

It outputs the following json

```json
 {
  "requirements": [
    {
      "name": "insurance_eligibility",
      "value": [
        "commercial",
        "health insurance exchanges",
        "federal employee plans",
        "state employee plans"
      ],
      "op": "All"
    },
    {
      "name": "insurance_ineligibility",
      "value": [
        "Medicaid",
        "Medicare",
        "VA",
        "DOD",
        "TRICARE",
        "federal",
        "state pharmaceutical assistance programs"
      ],
      "op": "NotIn"
    },
    {
      "name": "payment_eligibility",
      "value": "cash-paying patients",
      "op": "Equal"
    },
    {
      "name": "prescribed_for_indication",
      "value": "FDA-approved indication",
      "op": "Equal"
    },
    {
      "name": "patient_residence_eligibility",
      "value": [
        "US",
        "US territory"
      ],
      "op": "In"
    },
    {
      "name": "state_eligibility",
      "value": [],
      "op": "NotIn"
    }
  ]
}
```

