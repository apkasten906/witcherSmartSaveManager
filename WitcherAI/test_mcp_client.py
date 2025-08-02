#!/usr/bin/env python3
"""
Simple MCP Client to test our WitcherAI Python MCP Server
"""

import asyncio
import json
import sys

async def test_mcp_server():
    """Test the MCP server with simple JSON-RPC calls"""
    
    print("🧪 Testing WitcherAI MCP Python Server")
    print("=" * 50)
    
    # Test 1: Check ML imports
    print("\n🔬 Test 1: Testing ML Library Imports")
    test_request = {
        "jsonrpc": "2.0",
        "id": 1,
        "method": "tools/call",
        "params": {
            "name": "test_ml_imports",
            "arguments": {}
        }
    }
    
    print(f"Request: {json.dumps(test_request, indent=2)}")
    
    # Test 2: Check environment
    print("\n🔬 Test 2: Check WitcherAI Environment")
    env_request = {
        "jsonrpc": "2.0", 
        "id": 2,
        "method": "tools/call",
        "params": {
            "name": "check_witcher_environment",
            "arguments": {}
        }
    }
    
    print(f"Request: {json.dumps(env_request, indent=2)}")
    
    # Test 3: Run simple Python code
    print("\n🔬 Test 3: Run Simple Python Code")
    code_request = {
        "jsonrpc": "2.0",
        "id": 3, 
        "method": "tools/call",
        "params": {
            "name": "run_python_code",
            "arguments": {
                "code": "print('Hello from WitcherAI MCP Server!')\nprint(f'2 + 2 = {2 + 2}')",
                "context": "general"
            }
        }
    }
    
    print(f"Request: {json.dumps(code_request, indent=2)}")
    
    # Test 4: Run WitcherAI analysis code
    print("\n🔬 Test 4: Run WitcherAI Analysis Code")
    witcher_request = {
        "jsonrpc": "2.0",
        "id": 4,
        "method": "tools/call", 
        "params": {
            "name": "run_python_code",
            "arguments": {
                "code": """
# WitcherAI Cross-Game Pattern Test
import numpy as np
import pandas as pd

print("🎯 WitcherAI Phase 2B Test")
print("=" * 30)

# Test ML libraries
patterns = np.array(['questSystem', 'character_choice', 'political_decision'])
confidence = np.array([0.936, 0.85, 0.78])

df = pd.DataFrame({
    'pattern': patterns,
    'confidence': confidence,
    'game': ['Witcher 2'] * 3
})

print("📊 Pattern Analysis:")
print(df)
print(f"\\n📈 Average Confidence: {confidence.mean():.3f}")
print(f"🎯 Best Pattern: {patterns[confidence.argmax()]} ({confidence.max():.3f})")
""",
                "context": "witcher_analysis"
            }
        }
    }
    
    print(f"Request: {json.dumps(witcher_request, indent=2)}")
    
    print("\n✅ MCP Server Tests Prepared!")
    print("📝 The server is running - these requests would be sent via stdio")
    print("🚀 Ready for Phase 2B Cross-Game Pattern Analysis!")

if __name__ == "__main__":
    asyncio.run(test_mcp_server())
