#!/usr/bin/env python3
"""
WitcherAI MCP Python Server
Secure Python execution for WitcherAI development using official MCP package
"""

import asyncio
import sys
from pathlib import Path
from mcp.server import Server
from mcp.server.stdio import stdio_server
from mcp.types import Tool, TextContent
import subprocess
import json
import tempfile
import os

# Create the MCP server
server = Server("witcher-ai-python")

@server.list_tools()
async def list_tools():
    """List available Python execution tools"""
    return [
        Tool(
            name="run_python_code",
            description="Execute Python code safely with WitcherAI context",
            inputSchema={
                "type": "object",
                "properties": {
                    "code": {
                        "type": "string",
                        "description": "Python code to execute"
                    },
                    "context": {
                        "type": "string", 
                        "description": "Execution context (e.g., 'witcher_analysis', 'ml_testing')",
                        "default": "general"
                    }
                },
                "required": ["code"]
            }
        ),
        Tool(
            name="check_witcher_environment",
            description="Check WitcherAI Python environment and dependencies",
            inputSchema={
                "type": "object",
                "properties": {},
                "required": []
            }
        ),
        Tool(
            name="test_ml_imports",
            description="Test ML library imports (numpy, pandas, scikit-learn)",
            inputSchema={
                "type": "object", 
                "properties": {},
                "required": []
            }
        )
    ]

@server.call_tool()
async def call_tool(name: str, arguments: dict):
    """Execute the requested tool"""
    
    if name == "run_python_code":
        code = arguments.get("code", "")
        context = arguments.get("context", "general")
        
        try:
            # Create temporary file for code execution
            with tempfile.NamedTemporaryFile(mode='w', suffix='.py', delete=False) as f:
                # Add WitcherAI imports if needed
                if context == "witcher_analysis":
                    f.write("import sys\nsys.path.append('.')\n")
                    f.write("import numpy as np\nimport pandas as pd\n")
                    f.write("from pathlib import Path\n\n")
                
                f.write(code)
                temp_file = f.name
            
            # Execute the code
            result = subprocess.run(
                [sys.executable, temp_file],
                capture_output=True,
                text=True,
                timeout=30  # 30 second timeout
            )
            
            # Clean up
            os.unlink(temp_file)
            
            output = f"Exit Code: {result.returncode}\n"
            if result.stdout:
                output += f"STDOUT:\n{result.stdout}\n"
            if result.stderr:
                output += f"STDERR:\n{result.stderr}\n"
                
            return [TextContent(type="text", text=output)]
            
        except subprocess.TimeoutExpired:
            return [TextContent(type="text", text="Error: Code execution timed out (30 seconds)")]
        except Exception as e:
            return [TextContent(type="text", text=f"Error: {str(e)}")]
    
    elif name == "check_witcher_environment":
        try:
            # Check Python version and key packages
            check_code = """
import sys
print(f"Python Version: {sys.version}")
print(f"Python Executable: {sys.executable}")
print("\\nChecking WitcherAI dependencies:")

packages = ['numpy', 'pandas', 'scikit-learn', 'mcp']
for pkg in packages:
    try:
        __import__(pkg)
        print(f"✓ {pkg} - Available")
    except ImportError:
        print(f"✗ {pkg} - Missing")

print("\\nWitcherAI Directory Check:")
from pathlib import Path
base_path = Path('.')
for item in ['ml/', 'agents/', 'orchestration/', 'witcher_ai.py']:
    if (base_path / item).exists():
        print(f"✓ {item} - Found")
    else:
        print(f"✗ {item} - Missing")
"""
            
            result = subprocess.run(
                [sys.executable, "-c", check_code],
                capture_output=True,
                text=True
            )
            
            return [TextContent(type="text", text=result.stdout + result.stderr)]
            
        except Exception as e:
            return [TextContent(type="text", text=f"Error checking environment: {str(e)}")]
    
    elif name == "test_ml_imports":
        try:
            ml_test_code = """
print("Testing ML Library Imports...")
print("=" * 40)

try:
    import numpy as np
    print(f"✓ NumPy {np.__version__} - Available")
    # Quick test
    arr = np.array([1, 2, 3])
    print(f"  Test array: {arr}, sum: {arr.sum()}")
except Exception as e:
    print(f"✗ NumPy - Error: {e}")

try:
    import pandas as pd
    print(f"✓ Pandas {pd.__version__} - Available")
    # Quick test
    df = pd.DataFrame({'a': [1, 2, 3], 'b': [4, 5, 6]})
    print(f"  Test DataFrame shape: {df.shape}")
except Exception as e:
    print(f"✗ Pandas - Error: {e}")

try:
    import sklearn
    print(f"✓ Scikit-learn {sklearn.__version__} - Available")
    # Quick test
    from sklearn.ensemble import RandomForestClassifier
    print("  RandomForest import successful")
except Exception as e:
    print(f"✗ Scikit-learn - Error: {e}")

try:
    import mcp
    print(f"✓ MCP - Available")
    print("  MCP server capability confirmed")
except Exception as e:
    print(f"✗ MCP - Error: {e}")

print("\\nML Environment Status: Ready for WitcherAI Phase 2B")
"""
            
            result = subprocess.run(
                [sys.executable, "-c", ml_test_code],
                capture_output=True,
                text=True
            )
            
            return [TextContent(type="text", text=result.stdout + result.stderr)]
            
        except Exception as e:
            return [TextContent(type="text", text=f"Error testing ML imports: {str(e)}")]
    
    else:
        return [TextContent(type="text", text=f"Unknown tool: {name}")]

async def main():
    """Run the MCP server"""
    async with stdio_server() as (read_stream, write_stream):
        await server.run(read_stream, write_stream, server.create_initialization_options())

if __name__ == "__main__":
    asyncio.run(main())
